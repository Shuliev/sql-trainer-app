using Microsoft.Data.Sqlite;
using SqlTrainer.Core.Abstractions;

namespace SqlTrainer.Execution.Tests;

public sealed class SqliteQueryExecutorTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"sqltrainer-exec-tests-{Guid.NewGuid():N}");
    private readonly SqliteQueryExecutor _service;

    public SqliteQueryExecutorTests()
    {
        Directory.CreateDirectory(_directory);
        CreateDatabase("Fuhrpark", """
            CREATE TABLE kunde (id INTEGER PRIMARY KEY, name TEXT, preis REAL, bild BLOB);
            INSERT INTO kunde VALUES (1, 'Anna', 1.5, NULL), (2, NULL, 2.0, x'0102');
            """);
        _service = new SqliteQueryExecutor(new FolderLocator(_directory));
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public async Task ReturnsColumnsAndRawValuesByPosition()
    {
        var result = await _service.ExecuteAsync("Fuhrpark", "SELECT id, name, preis, bild, id FROM kunde ORDER BY id");

        Assert.True(result.IsSuccess);
        Assert.Equal(["id", "name", "preis", "bild", "id"], result.ResultSet!.Columns);
        Assert.Equal(2, result.ResultSet.Rows.Count);
        Assert.Equal([1L, "Anna", 1.5, null, 1L], result.ResultSet.Rows[0]);
        Assert.Equal(2L, result.ResultSet.Rows[1][0]);
        Assert.Null(result.ResultSet.Rows[1][1]);
        Assert.Equal(new byte[] { 1, 2 }, result.ResultSet.Rows[1][3]);
        Assert.False(result.ResultSet.IsTruncated);
    }

    [Fact]
    public async Task ReturnsEmptyResultWithColumnsWhenNoRowMatches()
    {
        var result = await _service.ExecuteAsync("Fuhrpark", "SELECT id, name FROM kunde WHERE id = 99");

        Assert.True(result.IsSuccess);
        Assert.Equal(["id", "name"], result.ResultSet!.Columns);
        Assert.Empty(result.ResultSet.Rows);
    }

    [Fact]
    public async Task CutsRowsAtTheLimitAndMarksTruncation()
    {
        var result = await _service.ExecuteAsync("Fuhrpark", Numbers(1500));

        Assert.True(result.IsSuccess);
        Assert.Equal(1000, result.ResultSet!.Rows.Count);
        Assert.True(result.ResultSet.IsTruncated);
    }

    [Fact]
    public async Task ExactlyTheLimitIsNotTruncated()
    {
        var result = await _service.ExecuteAsync("Fuhrpark", Numbers(1000));

        Assert.True(result.IsSuccess);
        Assert.Equal(1000, result.ResultSet!.Rows.Count);
        Assert.False(result.ResultSet.IsTruncated);
    }

    [Fact]
    public async Task MissingDatabaseFileReturnsNotFoundInsteadOfCreatingIt()
    {
        var result = await _service.ExecuteAsync("Unbekannt", "SELECT 1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Datenbank nicht gefunden.", result.ErrorMessage);
        Assert.Equal(["Fuhrpark"], Directory.EnumerateFileSystemEntries(_directory).Select(Path.GetFileName));
    }

    [Fact]
    public async Task InvalidDatabaseNameReturnsFailure()
    {
        var result = await _service.ExecuteAsync("..", "SELECT 1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Ungültiger Datenbankname.", result.ErrorMessage);
    }

    [Fact]
    public async Task SqliteErrorIsReturnedAsFailure()
    {
        var result = await _service.ExecuteAsync("Fuhrpark", "SELECT * FROM fehlt");

        Assert.False(result.IsSuccess);
        Assert.Contains("no such table: fehlt", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DELETE FROM kunde")]
    [InlineData("DROP TABLE kunde")]
    [InlineData("INSERT INTO kunde (id) VALUES (3)")]
    public async Task WritingStatementsFailAndLeaveDataUntouched(string sql)
    {
        var result = await _service.ExecuteAsync("Fuhrpark", sql);

        Assert.False(result.IsSuccess);
        var count = await _service.ExecuteAsync("Fuhrpark", "SELECT COUNT(*) FROM kunde");
        Assert.Equal(2L, count.ResultSet!.Rows[0][0]);
    }

    [Theory]
    [InlineData("SELECT 1; SELECT 2")]
    [InlineData("SELECT 1; SELECT 2;")]
    [InlineData("SELECT 1; DELETE FROM kunde")]
    [InlineData("SELECT 1; SELEKT 2")]
    public async Task SeveralStatementsAreRejected(string sql)
    {
        var result = await _service.ExecuteAsync("Fuhrpark", sql);

        Assert.False(result.IsSuccess);
        var count = await _service.ExecuteAsync("Fuhrpark", "SELECT COUNT(*) FROM kunde");
        Assert.Equal(2L, count.ResultSet!.Rows[0][0]);
    }

    [Fact]
    public async Task SeveralStatementsGiveTheOneStatementMessage()
    {
        var result = await _service.ExecuteAsync("Fuhrpark", "SELECT 1; SELECT 2");

        Assert.Equal("Es ist nur eine SQL-Anweisung erlaubt.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("SELECT 1;")]
    [InlineData("SELECT 1;  ")]
    [InlineData("SELECT 1; -- Kommentar")]
    [InlineData("SELECT 1 /* Kommentar */;\n")]
    public async Task SingleStatementWithTrailingSemicolonOrCommentIsAccepted(string sql)
    {
        var result = await _service.ExecuteAsync("Fuhrpark", sql);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1L, result.ResultSet!.Rows[0][0]);
    }

    [Fact]
    public async Task AttachCannotCreateAFileOutsideTheDatabase()
    {
        var target = Path.Combine(_directory, "neu.db").Replace('\\', '/');

        await _service.ExecuteAsync("Fuhrpark", $"ATTACH DATABASE '{target}' AS neu");

        Assert.False(File.Exists(Path.Combine(_directory, "neu.db")));
    }

    [Theory]
    [InlineData("PRAGMA query_only = 0")]
    [InlineData("PRAGMA journal_mode = WAL")]
    [InlineData("PRAGMA user_version = 7")]
    [InlineData("PRAGMA writable_schema = 1")]
    public async Task PragmasCannotChangeTheDatabaseFile(string sql)
    {
        var file = Path.Combine(_directory, "Fuhrpark", "datenbank.db");
        var before = await File.ReadAllBytesAsync(file);

        await _service.ExecuteAsync("Fuhrpark", sql);

        Assert.Equal(before, await File.ReadAllBytesAsync(file));
        Assert.Equal(["datenbank.db"], Directory.EnumerateFileSystemEntries(Path.Combine(_directory, "Fuhrpark")).Select(Path.GetFileName));
    }

    [Fact]
    public async Task ReadingPragmaReturnsRows()
    {
        var result = await _service.ExecuteAsync("Fuhrpark", "PRAGMA table_info(kunde)");

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.ResultSet!.Rows.Count);
    }

    [Fact]
    public async Task CancelledTokenReturnsFailureInsteadOfThrowing()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await _service.ExecuteAsync("Fuhrpark", "SELECT * FROM kunde", cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("Die Abfrage wurde abgebrochen.", result.ErrorMessage);
    }

    [Fact]
    public async Task DatabaseFileIsNotLockedAfterwards()
    {
        await _service.ExecuteAsync("Fuhrpark", "SELECT * FROM kunde");

        // Fails with an IOException while a pooled connection still holds the file (Windows).
        File.Delete(Path.Combine(_directory, "Fuhrpark", "datenbank.db"));
    }

    [Fact]
    public void ConstructorRejectsNullLocator()
    {
        Assert.Throws<ArgumentNullException>(() => new SqliteQueryExecutor(null!));
    }

    private static string Numbers(int count) =>
        $"WITH RECURSIVE n(i) AS (SELECT 1 UNION ALL SELECT i + 1 FROM n WHERE i < {count}) SELECT i FROM n";

    private void CreateDatabase(string name, string script)
    {
        var folder = Path.Combine(_directory, name);
        Directory.CreateDirectory(folder);
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(folder, "datenbank.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = script;
        command.ExecuteNonQuery();
    }

    /// <summary>Same layout as the real store: &lt;root&gt;/&lt;name&gt;/datenbank.db; rejects names that aren't a single path segment.</summary>
    private sealed class FolderLocator(string root) : IExerciseDatabaseLocator
    {
        public string GetFilePath(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName) || databaseName.StartsWith('.'))
            {
                throw new ArgumentException("Ungültiger Datenbankname.", nameof(databaseName));
            }

            return Path.Combine(root, databaseName, "datenbank.db");
        }
    }
}
