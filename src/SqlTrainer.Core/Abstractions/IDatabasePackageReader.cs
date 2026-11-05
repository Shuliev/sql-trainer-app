using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Reads and validates an uploaded import package (ZIP).</summary>
public interface IDatabasePackageReader
{
    /// <exception cref="InvalidDatabasePackageException">The package is invalid; the message is meant for the user.</exception>
    Task<DatabasePackage> ReadAsync(Stream zipStream, CancellationToken cancellationToken = default);
}
