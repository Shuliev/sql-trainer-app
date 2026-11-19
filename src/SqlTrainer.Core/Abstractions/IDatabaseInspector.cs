using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Builds a practice database from its setup script and describes its tables.</summary>
public interface IDatabaseInspector
{
    /// <summary>Runs the setup script and returns every table in creation order.</summary>
    /// <param name="setupScript">Contents of <c>datenbank.sql</c>.</param>
    /// <param name="maxSampleRows">Upper limit for <see cref="DatabaseTable.SampleRows"/> per table.</param>
    /// <param name="cancellationToken">Cancels the script and the reads.</param>
    /// <exception cref="InvalidDatabasePackageException">The script fails; the message is meant for the user.</exception>
    Task<IReadOnlyList<DatabaseTable>> InspectAsync(string setupScript, int maxSampleRows, CancellationToken cancellationToken = default);
}
