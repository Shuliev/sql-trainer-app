using Microsoft.Data.Sqlite;
using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Execution;

/// <summary>Runs the setup script in a private in-memory SQLite database and reads the tables back from it.</summary>
public sealed class SqliteDatabaseInspector : IDatabaseInspector
{
    public async Task<IReadOnlyList<DatabaseTable>> InspectAsync(string setupScript, int maxSampleRows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setupScript);
        ArgumentOutOfRangeException.ThrowIfNegative(maxSampleRows);

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);

        try
        {
            await using (var setup = connection.CreateCommand())
            {
                setup.CommandText = setupScript;
                await setup.ExecuteNonQueryAsync(cancellationToken);
            }

            var tables = new List<DatabaseTable>();
            foreach (var name in await ReadTableNamesAsync(connection, cancellationToken))
            {
                tables.Add(await ReadTableAsync(connection, name, maxSampleRows, cancellationToken));
            }
            return tables;
        }
        catch (SqliteException ex)
        {
            throw new InvalidDatabasePackageException($"Die Datei datenbank.sql konnte nicht ausgeführt werden: {ex.Message}", ex);
        }
    }

    private static async Task<List<string>> ReadTableNamesAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        // rowid order of sqlite_schema is the order the tables were created in.
        command.CommandText = "SELECT name FROM sqlite_schema WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY rowid";
        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            names.Add(reader.GetString(0));
        }
        return names;
    }

    private static async Task<DatabaseTable> ReadTableAsync(SqliteConnection connection, string table, int maxSampleRows, CancellationToken cancellationToken)
    {
        var quoted = Quote(table);

        var foreignKeyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT \"from\" FROM pragma_foreign_key_list($table)";
            command.Parameters.AddWithValue("$table", table);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                foreignKeyColumns.Add(reader.GetString(0));
            }
        }

        var columns = new List<TableColumn>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name, type, pk FROM pragma_table_info($table) ORDER BY cid";
            command.Parameters.AddWithValue("$table", table);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var name = reader.GetString(0);
                columns.Add(new TableColumn(name, reader.GetString(1), reader.GetInt64(2) > 0, foreignKeyColumns.Contains(name)));
            }
        }

        long rowCount;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT COUNT(*) FROM {quoted}";
            rowCount = (long)(await command.ExecuteScalarAsync(cancellationToken))!;
        }

        var rows = new List<IReadOnlyList<object?>>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT * FROM {quoted} LIMIT $limit";
            command.Parameters.AddWithValue("$limit", maxSampleRows);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var values = new object?[reader.FieldCount];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                rows.Add(values);
            }
        }

        var sampleRows = new ResultSet(columns.Select(c => c.Name).ToList(), rows, isTruncated: rowCount > rows.Count);
        return new DatabaseTable(table, columns, sampleRows, rowCount);
    }

    private static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
