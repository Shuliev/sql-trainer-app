using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Parses the contents of <c>aufgaben.txt</c>.</summary>
public interface IExerciseFileParser
{
    /// <exception cref="InvalidDatabasePackageException">The file doesn't match the format; the message is meant for the user.</exception>
    IReadOnlyList<ExerciseDefinition> Parse(string content);
}
