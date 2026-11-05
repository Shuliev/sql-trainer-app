namespace SqlTrainer.Core.Models;

/// <summary>Outcome of running a learner's query: either a result set or an error message.</summary>
public sealed class QueryResult
{
    private QueryResult(ResultSet? resultSet, string? errorMessage)
    {
        ResultSet = resultSet;
        ErrorMessage = errorMessage;
    }

    public ResultSet? ResultSet { get; }

    public string? ErrorMessage { get; }

    public bool IsSuccess => ResultSet is not null;

    public static QueryResult Success(ResultSet resultSet) =>
        new(resultSet ?? throw new ArgumentNullException(nameof(resultSet)), null);

    public static QueryResult Failure(string errorMessage) =>
        new(null, string.IsNullOrWhiteSpace(errorMessage)
            ? throw new ArgumentException("An error message is required.", nameof(errorMessage))
            : errorMessage);
}
