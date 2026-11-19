using System.Text;
using SqlTrainer.Application;
using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;
using SqlTrainer.UI.ViewModels;

namespace SqlTrainer.UI.Tests;

public class TaskViewModelTests
{
    private readonly FakeParser _parser = new();
    private readonly TaskViewModel _viewModel;

    public TaskViewModelTests()
    {
        var store = new FakeStore("Kino (1)");
        _viewModel = new TaskViewModel(new ExerciseService(store, _parser, new FakeInspector()));
    }

    [Fact]
    public async Task LoadAsync_LoadsExerciseAndPosition()
    {
        var loadingStates = new List<bool>();
        _viewModel.OnChange += () => loadingStates.Add(_viewModel.IsLoading);

        await _viewModel.LoadAsync("Kino (1)", "select", 3);

        Assert.Equal([true, false], loadingStates);
        Assert.Equal(3, _viewModel.Details!.Exercise.Number);
        Assert.Equal("SELECT", _viewModel.TopicDisplayName);
        Assert.Equal(2, _viewModel.Position);
        Assert.Equal(3, _viewModel.TopicExerciseCount);
        Assert.Equal("tasks/Kino%20%281%29/select/5", _viewModel.NextExerciseUrl);
        Assert.False(_viewModel.IsNotFound);
        Assert.Null(_viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_LastExerciseOfTopicHasNoNext()
    {
        await _viewModel.LoadAsync("Kino (1)", "SELECT", 5);

        Assert.Equal(3, _viewModel.Position);
        Assert.Null(_viewModel.NextExerciseUrl);
    }

    [Fact]
    public async Task LoadAsync_UnknownExerciseSetsNotFound()
    {
        await _viewModel.LoadAsync("Kino (1)", "join", 1);

        Assert.True(_viewModel.IsNotFound);
        Assert.Null(_viewModel.Details);
    }

    [Fact]
    public async Task LoadAsync_BrokenPackageShowsMessage()
    {
        _parser.Error = new InvalidDatabasePackageException("aufgaben.txt ist fehlerhaft");

        await _viewModel.LoadAsync("Kino (1)", "select", 1);

        Assert.Equal("aufgaben.txt ist fehlerhaft", _viewModel.ErrorMessage);
        Assert.False(_viewModel.IsNotFound);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadAsync_ResetsEditorState()
    {
        await _viewModel.LoadAsync("Kino (1)", "select", 1);
        _viewModel.SqlQuery = "SELECT 1";
        _viewModel.ToggleHint();
        _viewModel.ToggleSolution();
        _viewModel.CheckQuery();

        await _viewModel.LoadAsync("Kino (1)", "select", 3);

        Assert.Empty(_viewModel.SqlQuery);
        Assert.False(_viewModel.ShowHint);
        Assert.False(_viewModel.ShowSolution);
        Assert.Null(_viewModel.ResultMessage);
    }

    [Fact]
    public void ClearQuery_EmptiesEditorAndResult()
    {
        _viewModel.SqlQuery = "SELECT 1";
        _viewModel.CheckQuery();

        _viewModel.ClearQuery();

        Assert.Empty(_viewModel.SqlQuery);
        Assert.Null(_viewModel.ResultMessage);
    }

    [Theory]
    [InlineData(null, "NULL")]
    [InlineData(18.5, "18.5")]
    [InlineData(42L, "42")]
    [InlineData("Kino", "Kino")]
    public void FormatValue_UsesSqlStyle(object? value, string expected) =>
        Assert.Equal(expected, TaskViewModel.FormatValue(value));

    private sealed class FakeParser : IExerciseFileParser
    {
        public Exception? Error { get; set; }

        public IReadOnlyList<ExerciseDefinition> Parse(string content) => Error is null
            ?
            [
                new(1, Topic.Select, "A", "Tipp", "SELECT 1;"),
                new(3, Topic.Select, "B", "Tipp", "SELECT 3;"),
                new(5, Topic.Select, "C", "Tipp", "SELECT 5;"),
                new(6, Topic.Aggregate, "D", "Tipp", "SELECT 6;"),
            ]
            : throw Error;
    }

    private sealed class FakeInspector : IDatabaseInspector
    {
        public Task<IReadOnlyList<DatabaseTable>> InspectAsync(string setupScript, int maxSampleRows, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DatabaseTable>>([]);
    }

    private sealed class FakeStore(string databaseName) : IDatabasePackageStore
    {
        public string Location => "/fake";

        public IReadOnlyList<string> ListNames() => [databaseName];

        public bool Exists(string name) => name == databaseName;

        public Task SaveAsync(string name, DatabasePackage package, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<byte[]?> ReadFileAsync(string name, string fileName, CancellationToken cancellationToken = default) =>
            Task.FromResult<byte[]?>(Exists(name) ? Encoding.UTF8.GetBytes("x") : null);
    }
}
