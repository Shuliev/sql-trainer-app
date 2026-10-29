using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Stored practice databases (metadata in the app database).</summary>
public interface IExerciseDatabaseRepository
{
    Task<IReadOnlyList<ExerciseDatabase>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ExerciseDatabase?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
