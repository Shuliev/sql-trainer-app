using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using SqlTrainer.Core;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Storage.Tests;

public sealed class FileSystemDatabasePackageStoreTests : IDisposable
{
    private const string ScriptV1 = "CREATE TABLE kunde (id INTEGER PRIMARY KEY, name TEXT); INSERT INTO kunde (name) VALUES ('Anna');";
    private const string ScriptV2 = "CREATE TABLE auto (id INTEGER PRIMARY KEY);";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"sqltrainer-tests-{Guid.NewGuid():N}");
    private readonly FileSystemDatabasePackageStore _store;

    public FileSystemDatabasePackageStoreTests()
    {
        _store = new FileSystemDatabasePackageStore(_directory, NullLogger<FileSystemDatabasePackageStore>.Instance);
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public void ConstructorCreatesWritableDirectory()
    {
        Assert.Equal(_directory, _store.Location);
        Assert.True(Directory.Exists(_directory));
        Assert.Empty(_store.ListNames());
    }

    [Fact]
    public async Task SaveAsyncWritesFilesAndListsDatabase()
    {
        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", ScriptV1)));

        Assert.True(_store.Exists("Fuhrpark"));
        Assert.Equal(["Fuhrpark"], _store.ListNames());
        Assert.Equal(ScriptV1, await File.ReadAllTextAsync(Path.Combine(_directory, "Fuhrpark", "datenbank.sql")));
    }

    [Fact]
    public async Task SaveAsyncCreatesDatabaseFileFromScript()
    {
        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", ScriptV1)));

        Assert.Equal("Anna", QueryScalar(_store.GetFilePath("Fuhrpark"), "SELECT name FROM kunde"));
    }

    [Fact]
    public async Task SaveAsyncReleasesDatabaseFile()
    {
        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", ScriptV1)));

        // Fails with an IOException while a pooled connection still holds the file (Windows).
        File.Delete(_store.GetFilePath("Fuhrpark"));
    }

    [Fact]
    public async Task SaveAsyncExistingDatabaseReplacesIt()
    {
        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", ScriptV1), ("alt.txt", "x")));

        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", ScriptV2)));

        Assert.Equal(
            ["datenbank.db", "datenbank.sql"],
            Directory.GetFiles(Path.Combine(_directory, "Fuhrpark")).Select(Path.GetFileName).Order());
        Assert.Equal(ScriptV2, await File.ReadAllTextAsync(Path.Combine(_directory, "Fuhrpark", "datenbank.sql")));
        Assert.Equal(0L, QueryScalar(_store.GetFilePath("Fuhrpark"), "SELECT COUNT(*) FROM auto"));
    }

    [Fact]
    public async Task SaveAsyncFailingScriptThrowsWithSqliteMessageAndKeepsOldDatabase()
    {
        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", ScriptV1)));

        var exception = await Assert.ThrowsAsync<InvalidDatabasePackageException>(() =>
            _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", "CREATE TABLE ok (id INTEGER); INSERT INTO fehlt VALUES (1);"))));

        Assert.Contains("no such table: fehlt", exception.Message, StringComparison.Ordinal);
        Assert.Equal(["Fuhrpark"], Directory.EnumerateFileSystemEntries(_directory).Select(Path.GetFileName));
        Assert.Equal(ScriptV1, await File.ReadAllTextAsync(Path.Combine(_directory, "Fuhrpark", "datenbank.sql")));
        Assert.Equal("Anna", QueryScalar(_store.GetFilePath("Fuhrpark"), "SELECT name FROM kunde"));
    }

    [Fact]
    public async Task SaveAsyncFailingScriptForNewDatabaseLeavesNothing()
    {
        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() =>
            _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", "SELEKT 1;"))));

        Assert.Empty(Directory.EnumerateFileSystemEntries(_directory));
    }

    [Fact]
    public async Task SaveAsyncScriptIsRunInOneTransaction()
    {
        // The first statement would succeed on its own; the whole script must roll back, so nothing may remain.
        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() =>
            _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", "CREATE TABLE a (id INTEGER); CREATE TABLE a (id INTEGER);"))));

        Assert.Empty(Directory.EnumerateFileSystemEntries(_directory));
    }

    [Fact]
    public async Task SaveAsyncWithoutScriptThrows()
    {
        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _store.SaveAsync("Fuhrpark", Package(("beschreibung.txt", "x"))));

        Assert.Empty(Directory.EnumerateFileSystemEntries(_directory));
    }

    [Theory]
    [InlineData("")]
    [InlineData("..")]
    [InlineData(".hidden")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    public async Task SaveAsyncInvalidNameThrowsAndWritesNothing(string name)
    {
        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _store.SaveAsync(name, Package(("datenbank.sql", ScriptV1))));

        Assert.Empty(Directory.EnumerateFileSystemEntries(_directory));
    }

    [Fact]
    public async Task SaveAsyncInvalidFileNameThrowsAndLeavesNoTemporaryFolder()
    {
        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _store.SaveAsync("Fuhrpark", Package(("../evil.txt", "x"))));

        Assert.Empty(Directory.EnumerateFileSystemEntries(_directory));
        Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(_directory)!, "evil.txt")));
    }

    [Fact]
    public void ListNamesIgnoresHiddenFolders()
    {
        Directory.CreateDirectory(Path.Combine(_directory, ".import-leftover"));

        Assert.Empty(_store.ListNames());
    }

    [Fact]
    public void GetFilePathReturnsDatabaseFileInsideTheDatabaseFolder()
    {
        Assert.Equal(Path.Combine(_directory, "Fuhrpark", "datenbank.db"), _store.GetFilePath("Fuhrpark"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("..")]
    [InlineData(".hidden")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    public void GetFilePathInvalidNameThrows(string name)
    {
        Assert.Throws<ArgumentException>(() => _store.GetFilePath(name));
    }

    private static DatabasePackage Package(params (string Name, string Content)[] files) =>
        new(files.ToDictionary(file => file.Name, file => System.Text.Encoding.UTF8.GetBytes(file.Content)));

    private static object? QueryScalar(string databaseFile, string sql)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = databaseFile, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }
}
