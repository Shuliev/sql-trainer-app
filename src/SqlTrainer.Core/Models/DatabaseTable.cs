namespace SqlTrainer.Core.Models;

/// <summary>Structure and sample rows of one table of a practice database.</summary>
/// <param name="Name">Table name as created by the setup script.</param>
/// <param name="Columns">Columns in declaration order.</param>
/// <param name="SampleRows">The first rows of the table (all columns); may be fewer than <paramref name="RowCount"/>.</param>
/// <param name="RowCount">Total number of rows in the table.</param>
public sealed record DatabaseTable(string Name, IReadOnlyList<TableColumn> Columns, ResultSet SampleRows, long RowCount);

/// <summary>A column with its declared type, e.g. "INTEGER" or "VARCHAR(100)".</summary>
public sealed record TableColumn(string Name, string Type, bool IsPrimaryKey, bool IsForeignKey);
