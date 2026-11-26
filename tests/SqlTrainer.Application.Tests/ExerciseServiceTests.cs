using System.Text;
using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Application.Tests;

public class ExerciseServiceTests
{
    private static readonly ExerciseDefinition[] _exercises =
    [
        new(2, Topic.Select, "B", "", "SELECT 2;"),
        new(1, Topic.Select, "A", "", "SELECT 1;"),
        new(6, Topic.Aggregate, "C", "", "SELECT 6;"),
    ];

    private readonly FakeStore _store = new();
    private readonly FakeInspector _inspector = new();
    private readonly ExerciseService _service;

    public ExerciseServiceTests()
    {
        _store.Files["Kino (1)/aufgaben.txt"] = Encoding.UTF8.GetBytes("aufgaben");
        _store.Files["Kino (1)/datenbank.sql"] = Encoding.UTF8.GetBytes("\uFEFFCREATE TABLE film (id);");
        _service = new ExerciseService(_store, new FakeParser(), _inspector);
    }

    [Fact]
    public async Task GetExerciseAsyncReturnsExerciseWithTopicExercisesAndTables()
    {
        var details = await _service.GetExerciseAsync("Kino (1)", "select", 2);

        Assert.NotNull(details);
        Assert.Equal("Kino (1)", details.DatabaseName);
        Assert.Same(_exercises[0], details.Exercise);
        Assert.Equal([1, 2], details.TopicExercises.Select(e => e.Number));
        Assert.Same(_inspector.Tables, details.Tables);
        Assert.Equal("CREATE TABLE film (id);", _inspector.Script);
        Assert.Equal(ExerciseService.SampleRowLimit, _inspector.MaxRows);
    }

    [Theory]
    [InlineData("Unbekannt", "select", 1)]
    [InlineData("Kino (1)", "union", 1)]
    [InlineData("Kino (1)", "select", 6)]
    [InlineData("Kino (1)", "aggregat", 99)]
    public async Task GetExerciseAsyncUnknownDatabaseTopicOrNumberReturnsNull(string databaseName, string topic, int number) =>
        Assert.Null(await _service.GetExerciseAsync(databaseName, topic, number));

    [Fact]
    public async Task GetErDiagramAsyncReturnsStoredPng()
    {
        byte[] png = [0x89, 0x50, 0x4E, 0x47];
        _store.Files["Kino (1)/er_diagramm.png"] = png;

        Assert.Equal(png, await _service.GetErDiagramAsync("Kino (1)"));
        Assert.Null(await _service.GetErDiagramAsync("Unbekannt"));
    }

    [Fact]
    public async Task GetExerciseAsyncMissingFileThrows()
    {
        _store.Files.Remove("Kino (1)/datenbank.sql");

        var ex = await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _service.GetExerciseAsync("Kino (1)", "select", 1));

        Assert.Contains("datenbank.sql", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetExerciseAsyncNonUtf8FileThrows()
    {
        _store.Files["Kino (1)/aufgaben.txt"] = [0xC3, 0x28];

        var ex = await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _service.GetExerciseAsync("Kino (1)", "select", 1));

        Assert.Contains("UTF-8", ex.Message, StringComparison.Ordinal);
    }

    private sealed class FakeParser : IExerciseFileParser
    {
        public IReadOnlyList<ExerciseDefinition> Parse(string content) => _exercises;
    }

    private sealed class FakeInspector : IDatabaseInspector
    {
        public IReadOnlyList<DatabaseTable> Tables { get; } = [new("film", [], new ResultSet([], []), 0)];

        public string? Script { get; private set; }

        public int MaxRows { get; private set; }

        public Task<IReadOnlyList<DatabaseTable>> InspectAsync(string setupScript, int maxSampleRows, CancellationToken cancellationToken = default)
        {
            Script = setupScript;
            MaxRows = maxSampleRows;
            return Task.FromResult(Tables);
        }
    }

    private sealed class FakeStore : IDatabasePackageStore
    {
        public Dictionary<string, byte[]> Files { get; } = [];

        public string Location => "/fake";

        public IReadOnlyList<string> ListNames() => [.. Files.Keys.Select(key => key.Split('/')[0]).Distinct()];

        public bool Exists(string databaseName) => ListNames().Contains(databaseName);

        public Task SaveAsync(string databaseName, DatabasePackage package, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<byte[]?> ReadFileAsync(string databaseName, string fileName, CancellationToken cancellationToken = default) =>
            Task.FromResult(Files.GetValueOrDefault($"{databaseName}/{fileName}"));
    }
}
