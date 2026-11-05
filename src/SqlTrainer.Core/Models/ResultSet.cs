namespace SqlTrainer.Core.Models;

/// <summary>
/// Tabular result of one query. Values are the raw provider values
/// (for SQLite: <see cref="long"/>, <see cref="double"/>, <see cref="string"/>, <see cref="byte"/>[] or <c>null</c>).
/// </summary>
public sealed class ResultSet
{
    public ResultSet(IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<object?>> rows, bool isTruncated = false)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);

        for (var i = 0; i < rows.Count; i++)
        {
            if (rows[i].Count != columns.Count)
            {
                throw new ArgumentException(
                    $"Row {i} has {rows[i].Count} values, but there are {columns.Count} columns.", nameof(rows));
            }
        }

        Columns = columns;
        Rows = rows;
        IsTruncated = isTruncated;
    }

    /// <summary>Column names as returned by the query (aliases included).</summary>
    public IReadOnlyList<string> Columns { get; }

    /// <summary>Rows in the order the database returned them; each row has one value per column.</summary>
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; }

    /// <summary>True if the row limit cut off further rows.</summary>
    public bool IsTruncated { get; }
}
