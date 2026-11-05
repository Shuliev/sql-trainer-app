using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Application.Tests;

public class DatabaseManagementServiceTests
{
    private readonly FakeReader _reader = new();
    private readonly FakeStore _store = new();

    [Fact]
    public async Task ImportAsyncValidPackageSavesUnderGivenName()
    {
        var service = new DatabaseManagementService(_reader, _store);

        await service.ImportAsync(new MemoryStream(), "Fuhrpark");

        Assert.Same(_reader.Package, _store.Saved["Fuhrpark"]);
        Assert.Equal(["Fuhrpark"], service.ListDatabases());
        Assert.True(service.Exists("Fuhrpark"));
    }

    [Fact]
    public async Task ImportAsyncInvalidPackageSavesNothing()
    {
        _reader.Error = new InvalidDatabasePackageException("kaputt");
        var service = new DatabaseManagementService(_reader, _store);

        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => service.ImportAsync(new MemoryStream(), "Fuhrpark"));

        Assert.Empty(_store.Saved);
    }

    private sealed class FakeReader : IDatabasePackageReader
    {
        public DatabasePackage Package { get; } = new(new Dictionary<string, byte[]>());

        public Exception? Error { get; set; }

        public Task<DatabasePackage> ReadAsync(Stream zipStream, CancellationToken cancellationToken = default) =>
            Error is null ? Task.FromResult(Package) : Task.FromException<DatabasePackage>(Error);
    }

    private sealed class FakeStore : IDatabasePackageStore
    {
        public Dictionary<string, DatabasePackage> Saved { get; } = [];

        public string Location => "/fake";

        public IReadOnlyList<string> ListNames() => [.. Saved.Keys];

        public bool Exists(string databaseName) => Saved.ContainsKey(databaseName);

        public Task SaveAsync(string databaseName, DatabasePackage package, CancellationToken cancellationToken = default)
        {
            Saved[databaseName] = package;
            return Task.CompletedTask;
        }
    }
}
