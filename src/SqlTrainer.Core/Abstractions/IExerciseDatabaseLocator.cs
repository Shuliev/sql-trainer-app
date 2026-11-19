namespace SqlTrainer.Core.Abstractions;

/// <summary>Finds the SQLite file of a stored practice database.</summary>
public interface IExerciseDatabaseLocator
{
    /// <summary>Path of the SQLite file of the database stored under <paramref name="databaseName"/>.</summary>
    /// <exception cref="ArgumentException">The name is not a valid database name.</exception>
    string GetFilePath(string databaseName);
}
