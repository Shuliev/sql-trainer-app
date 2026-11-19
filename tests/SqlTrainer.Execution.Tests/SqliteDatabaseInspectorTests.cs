using SqlTrainer.Core;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Execution.Tests;

public class SqliteDatabaseInspectorTests
{
    private const string Script = """
        PRAGMA foreign_keys = ON;
        CREATE TABLE autor (autor_id INTEGER PRIMARY KEY, name VARCHAR(100) NOT NULL);
        CREATE TABLE buch (buch_id INTEGER PRIMARY KEY, autor_id INTEGER, preis DECIMAL(10,2),
            FOREIGN KEY (autor_id) REFERENCES autor(autor_id));
        INSERT INTO autor VALUES (1, 'Michael Ende'), (2, 'Jules Verne'), (3, 'Astrid Lindgren');
        INSERT INTO buch VALUES (1, 1, 18.5), (2, NULL, 14);
        """;

    private readonly SqliteDatabaseInspector _inspector = new();

    [Fact]
    public async Task InspectAsyncReturnsTablesInCreationOrderWithColumns()
    {
        var tables = await _inspector.InspectAsync(Script, 5);

        Assert.Equal(["autor", "buch"], tables.Select(t => t.Name));
        Assert.Equal(
            [
                new TableColumn("buch_id", "INTEGER", IsPrimaryKey: true, IsForeignKey: false),
                new TableColumn("autor_id", "INTEGER", IsPrimaryKey: false, IsForeignKey: true),
                new TableColumn("preis", "DECIMAL(10,2)", IsPrimaryKey: false, IsForeignKey: false),
            ],
            tables[1].Columns);
    }

    [Fact]
    public async Task InspectAsyncLimitsSampleRowsAndCountsAllRows()
    {
        var autor = (await _inspector.InspectAsync(Script, 2))[0];

        Assert.Equal(3, autor.RowCount);
        Assert.True(autor.SampleRows.IsTruncated);
        Assert.Equal(["autor_id", "name"], autor.SampleRows.Columns);
        Assert.Equal([[1L, "Michael Ende"], [2L, "Jules Verne"]], autor.SampleRows.Rows.Select(r => r.ToArray()));
    }

    [Fact]
    public async Task InspectAsyncKeepsNullAndRealValues()
    {
        var buch = (await _inspector.InspectAsync(Script, 5))[1];

        Assert.False(buch.SampleRows.IsTruncated);
        Assert.Equal(18.5, buch.SampleRows.Rows[0][2]);
        Assert.Null(buch.SampleRows.Rows[1][1]);
    }

    [Fact]
    public async Task InspectAsyncBrokenScriptThrowsWithMessage()
    {
        var ex = await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _inspector.InspectAsync("CREATE TABLE (", 5));

        Assert.Contains("datenbank.sql", ex.Message, StringComparison.Ordinal);
    }

    /// <summary>Every bundled sample database must run and contain data.</summary>
    [Fact]
    public async Task InspectAsyncBundledSampleDatabases()
    {
        var files = SampleDatabases.Files("datenbank.sql");
        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var tables = await _inspector.InspectAsync(await File.ReadAllTextAsync(file), 5);

            Assert.NotEmpty(tables);
            Assert.All(tables, table => Assert.True(table.RowCount > 0, $"{file}: {table.Name} is empty"));
        }
    }
}
