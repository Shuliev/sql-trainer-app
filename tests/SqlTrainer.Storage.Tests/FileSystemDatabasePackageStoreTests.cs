using Microsoft.Extensions.Logging.Abstractions;
using SqlTrainer.Core;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Storage.Tests;

public sealed class FileSystemDatabasePackageStoreTests : IDisposable
{
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
        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", "v1")));

        Assert.True(_store.Exists("Fuhrpark"));
        Assert.Equal(["Fuhrpark"], _store.ListNames());
        Assert.Equal("v1", await File.ReadAllTextAsync(Path.Combine(_directory, "Fuhrpark", "datenbank.sql")));
    }

    [Fact]
    public async Task SaveAsyncExistingDatabaseReplacesIt()
    {
        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", "v1"), ("alt.txt", "x")));

        await _store.SaveAsync("Fuhrpark", Package(("datenbank.sql", "v2")));

        Assert.Equal(["datenbank.sql"], Directory.GetFiles(Path.Combine(_directory, "Fuhrpark")).Select(Path.GetFileName));
        Assert.Equal("v2", await File.ReadAllTextAsync(Path.Combine(_directory, "Fuhrpark", "datenbank.sql")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("..")]
    [InlineData(".hidden")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    public async Task SaveAsyncInvalidNameThrowsAndWritesNothing(string name)
    {
        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _store.SaveAsync(name, Package(("datenbank.sql", "x"))));

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

    private static DatabasePackage Package(params (string Name, string Content)[] files) =>
        new(files.ToDictionary(file => file.Name, file => System.Text.Encoding.UTF8.GetBytes(file.Content)));
}
