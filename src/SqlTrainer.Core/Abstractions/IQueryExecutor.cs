using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Runs a single read-only query against a practice database.</summary>
public interface IQueryExecutor
{
    Task<QueryResult> ExecuteAsync(string databaseName, string sql, CancellationToken cancellationToken = default);
}
