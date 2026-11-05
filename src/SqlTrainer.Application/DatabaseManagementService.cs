using SqlTrainer.Core.Abstractions;

namespace SqlTrainer.Application;

/// <summary>Use cases of the database management screen: list stored databases and import a package.</summary>
public sealed class DatabaseManagementService(IDatabasePackageReader reader, IDatabasePackageStore store)
{
    public string StorageLocation => store.Location;

    public IReadOnlyList<string> ListDatabases() => store.ListNames();

    public bool Exists(string databaseName) => store.Exists(databaseName);

    /// <summary>Validates the ZIP and stores it under <paramref name="databaseName"/>, replacing an existing database.</summary>
    /// <exception cref="Core.InvalidDatabasePackageException">The package is invalid; the message is meant for the user.</exception>
    public async Task ImportAsync(Stream zipStream, string databaseName, CancellationToken cancellationToken = default)
    {
        var package = await reader.ReadAsync(zipStream, cancellationToken);
        await store.SaveAsync(databaseName, package, cancellationToken);
    }
}
