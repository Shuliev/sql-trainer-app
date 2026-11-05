using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Stored exercises; they can be edited and added in the app.</summary>
public interface IExerciseRepository
{
    Task<IReadOnlyList<Exercise>> GetByDatabaseAsync(Guid databaseId, CancellationToken cancellationToken = default);

    Task<Exercise?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Adds the exercise or replaces the stored one with the same <see cref="Exercise.Id"/>.</summary>
    Task SaveAsync(Exercise exercise, CancellationToken cancellationToken = default);
}
