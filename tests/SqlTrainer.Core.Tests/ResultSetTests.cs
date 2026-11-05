using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Tests;

public class ResultSetTests
{
    [Fact]
    public void Constructor_KeepsColumnsRowsAndTruncation()
    {
        var resultSet = new ResultSet(["id", "name"], [[1L, "Anna"], [2L, null]], isTruncated: true);

        Assert.Equal(["id", "name"], resultSet.Columns);
        Assert.Equal(2, resultSet.Rows.Count);
        Assert.Null(resultSet.Rows[1][1]);
        Assert.True(resultSet.IsTruncated);
    }

    [Fact]
    public void Constructor_AllowsEmptyResult()
    {
        var resultSet = new ResultSet(["id"], []);

        Assert.Empty(resultSet.Rows);
        Assert.False(resultSet.IsTruncated);
    }

    [Fact]
    public void Constructor_RejectsRowWithWrongValueCount()
    {
        var exception = Assert.Throws<ArgumentException>(() => new ResultSet(["id", "name"], [[1L, "Anna"], [2L]]));

        Assert.Contains("Row 1", exception.Message, StringComparison.Ordinal);
    }
}
