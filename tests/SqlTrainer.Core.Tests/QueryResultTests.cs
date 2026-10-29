using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Tests;

public class QueryResultTests
{
    [Fact]
    public void Success_HoldsResultSet()
    {
        var resultSet = new ResultSet(["id"], [[1L]]);

        var result = QueryResult.Success(resultSet);

        Assert.True(result.IsSuccess);
        Assert.Same(resultSet, result.ResultSet);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_HoldsErrorMessage()
    {
        var result = QueryResult.Failure("no such table: kunde");

        Assert.False(result.IsSuccess);
        Assert.Null(result.ResultSet);
        Assert.Equal("no such table: kunde", result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Failure_RequiresMessage(string message)
    {
        Assert.Throws<ArgumentException>(() => QueryResult.Failure(message));
    }
}
