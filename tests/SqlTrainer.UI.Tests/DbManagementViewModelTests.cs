using SqlTrainer.Application;
using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;
using SqlTrainer.UI.ViewModels;

namespace SqlTrainer.UI.Tests;

public class DbManagementViewModelTests
{
    private readonly FakeReader _reader = new();
    private readonly FakeStore _store = new();
    private readonly DbManagementViewModel _viewModel;

    public DbManagementViewModelTests()
    {
        _viewModel = new DbManagementViewModel(new DatabaseManagementService(_reader, _store));
    }

    [Fact]
    public void Load_ListsStoredDatabasesAndRaisesOnChange()
    {
        _store.Names.Add("Bibliothek");
        var changes = 0;
        _viewModel.OnChange += () => changes++;

        _viewModel.Load();

        Assert.Equal(["Bibliothek"], _viewModel.Databases);
        Assert.Equal("/fake", _viewModel.StorageLocation);
        Assert.Equal(1, changes);
    }

    [Theory]
    [InlineData("db.txt", 10, "ZIP")]
    [InlineData("db.zip", DatabasePackageFormat.MaxPackageBytes + 1, "zu groß")]
    public void ValidateFile_RejectsWrongTypeOrSize(string fileName, long size, string expected) =>
        Assert.Contains(expected, DbManagementViewModel.ValidateFile(fileName, size), StringComparison.Ordinal);

    [Fact]
    public void ValidateFile_AcceptsZip() => Assert.Null(DbManagementViewModel.ValidateFile("db.ZIP", 10));

    [Fact]
    public async Task UploadAsync_Success_RefreshesListAndResetsBusyState()
    {
        var busyStates = new List<bool>();
        _viewModel.OnChange += () => busyStates.Add(_viewModel.IsUploading);

        var result = await _viewModel.UploadAsync(new MemoryStream(), "Fuhrpark");

        Assert.True(result.Succeeded);
        Assert.Equal(["Fuhrpark"], _viewModel.Databases);
        Assert.Equal([true, false], busyStates);
    }

    [Fact]
    public async Task UploadAsync_InvalidPackage_ReturnsMessage()
    {
        _reader.Error = new InvalidDatabasePackageException("Die Datei \"aufgaben.txt\" fehlt in der ZIP-Datei.");

        var result = await _viewModel.UploadAsync(new MemoryStream(), "Fuhrpark");

        Assert.False(result.Succeeded);
        Assert.Equal(_reader.Error.Message, result.Message);
        Assert.False(_viewModel.IsUploading);
        Assert.Empty(_store.Names);
    }

    private sealed class FakeReader : IDatabasePackageReader
    {
        public Exception? Error { get; set; }

        public Task<DatabasePackage> ReadAsync(Stream zipStream, CancellationToken cancellationToken = default) =>
            Error is null
                ? Task.FromResult(new DatabasePackage(new Dictionary<string, byte[]>()))
                : Task.FromException<DatabasePackage>(Error);
    }

    private sealed class FakeStore : IDatabasePackageStore
    {
        public List<string> Names { get; } = [];

        public string Location => "/fake";

        public IReadOnlyList<string> ListNames() => [.. Names];

        public bool Exists(string databaseName) => Names.Contains(databaseName);

        public Task SaveAsync(string databaseName, DatabasePackage package, CancellationToken cancellationToken = default)
        {
            Names.Add(databaseName);
            return Task.CompletedTask;
        }
    }
}
