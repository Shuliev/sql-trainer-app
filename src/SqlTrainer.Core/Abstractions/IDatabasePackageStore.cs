using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Stores imported database packages on the local machine.</summary>
public interface IDatabasePackageStore
{
    /// <summary>Directory the packages are stored in (shown to the user).</summary>
    string Location { get; }

    IReadOnlyList<string> ListNames();

    bool Exists(string databaseName);

    /// <summary>Saves the package under <paramref name="databaseName"/>, replacing an existing one with the same name.</summary>
    Task SaveAsync(string databaseName, DatabasePackage package, CancellationToken cancellationToken = default);
}
