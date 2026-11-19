using Microsoft.Data.Sqlite;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Execution;

public sealed class SqliteQueryExecutor : IQueryExecutor
{
    // Plan 3.4: at most this many rows are returned; further rows only set IsTruncated.
    private const int _maxRows = 1000;

    private readonly IExerciseDatabaseLocator _locator;

    public SqliteQueryExecutor(IExerciseDatabaseLocator databaseLocator)
    {
        ArgumentNullException.ThrowIfNull(databaseLocator);

        _locator = databaseLocator;
    }

    public async Task<QueryResult> ExecuteAsync(string databaseName, string sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sql);

        string databaseFile;
        try
        {
            databaseFile = _locator.GetFilePath(databaseName);
        }
        catch (ArgumentException)
        {
            return QueryResult.Failure("Ungültiger Datenbankname.");
        }

        // Check first, otherwise SQLite reports a missing file as "no such table" or "unable to open".
        if (!File.Exists(databaseFile))
        {
            return QueryResult.Failure("Datenbank nicht gefunden.");
        }

        // Read-only: even a DELETE/DROP typed by a learner fails. No pooling, so no connection keeps the file locked.
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseFile,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false,
        }.ToString();

        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqliteCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        return ReadResult(reader);
                    }
                }
            }
        }
        catch (SqliteException ex)
        {
            return QueryResult.Failure(ex.Message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The caller cancelled (e.g. left the page): report it as a result instead of an exception.
            return QueryResult.Failure("Die Abfrage wurde abgebrochen.");
        }
    }

    // Turns the reader's output into our ResultSet: columns by position (aliases and duplicate
    // names kept as the query returns them), raw values (long, double, string, byte[] or null,
    // so NULL stays distinguishable from '' and numbers from text), at most _maxRows rows.
    private static QueryResult ReadResult(SqliteDataReader reader)
    {
        var columns = new string[reader.FieldCount];
        for (var i = 0; i < columns.Length; i++)
        {
            columns[i] = reader.GetName(i);
        }

        var rows = new List<IReadOnlyList<object?>>();
        while (rows.Count < _maxRows && reader.Read())
        {
            var row = new object?[columns.Length];
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            rows.Add(row);
        }

        // The limit is reached: if one more row exists, the result was cut off.
        var isTruncated = rows.Count == _maxRows && reader.Read();

        // Plan 3.4: exactly one statement per input. The reader only returns the first one's result, so
        // without this check "SELECT 1; SELECT 2" would look like a success. A single trailing ';' is fine.
        if (reader.NextResult())
        {
            return QueryResult.Failure("Es ist nur eine SQL-Anweisung erlaubt.");
        }

        return QueryResult.Success(new ResultSet(columns, rows, isTruncated));
    }
}
