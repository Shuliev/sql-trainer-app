using System.Globalization;
using SqlTrainer.Application;
using SqlTrainer.Core;
using SqlTrainer.Core.Models;

namespace SqlTrainer.UI.ViewModels;

/// <summary>State of the exercise screen: the loaded exercise, the editor and hint/solution toggles.</summary>
public sealed class TaskViewModel(ExerciseService exercises) : ViewModelBase
{
    private int _loadVersion;

    public bool IsLoading { get; private set; }

    /// <summary>True if the database, topic or exercise from the URL doesn't exist.</summary>
    public bool IsNotFound { get; private set; }

    public string? ErrorMessage { get; private set; }

    public ExerciseDetails? Details { get; private set; }

    public string SqlQuery { get; set; } = string.Empty;

    public bool ShowHint { get; private set; }

    public bool ShowSolution { get; private set; }

    public string? ResultMessage { get; private set; }

    public bool ShowErDiagram { get; private set; }

    /// <summary>ER diagram as <c>data:</c> URL; <c>null</c> until it was opened once for the current database.</summary>
    public string? ErDiagramUrl { get; private set; }

    /// <summary>Message shown in the diagram dialog when the diagram couldn't be loaded.</summary>
    public string? ErDiagramError { get; private set; }

    public string TopicDisplayName => Details is null ? string.Empty : TopicNames.GetDisplayName(Details.Exercise.Topic);

    /// <summary>1-based position of the exercise within its topic.</summary>
    public int Position => Details is null ? 0 : IndexInTopic(Details) + 1;

    public int TopicExerciseCount => Details?.TopicExercises.Count ?? 0;

    /// <summary>URL of the task list of the current database, e.g. <c>tasks/Kino%20(1)/</c>.</summary>
    public string? TaskListUrl => Details is null ? null : GetTaskListUrl(Details.DatabaseName);

    /// <summary>URL of the previous exercise of the same topic; <c>null</c> on the first one.</summary>
    public string? PreviousExerciseUrl
    {
        get
        {
            if (Details is null)
            {
                return null;
            }
            var previous = IndexInTopic(Details) - 1;
            return previous >= 0
                ? GetExerciseUrl(Details.DatabaseName, Details.TopicExercises[previous])
                : null;
        }
    }

    /// <summary>URL of the next exercise of the same topic; <c>null</c> on the last one.</summary>
    public string? NextExerciseUrl
    {
        get
        {
            if (Details is null)
            {
                return null;
            }
            var next = IndexInTopic(Details) + 1;
            return next < Details.TopicExercises.Count
                ? GetExerciseUrl(Details.DatabaseName, Details.TopicExercises[next])
                : null;
        }
    }

    public async Task LoadAsync(string databaseName, string topicKey, int number, CancellationToken cancellationToken = default)
    {
        // A newer navigation may start before this one finished; only the latest one may update the state.
        var version = ++_loadVersion;
        IsLoading = true;
        IsNotFound = false;
        ErrorMessage = null;
        SqlQuery = string.Empty;
        ShowHint = false;
        ShowSolution = false;
        ResultMessage = null;
        NotifyStateChanged();

        ExerciseDetails? details = null;
        string? error = null;
        try
        {
            details = await exercises.GetExerciseAsync(databaseName, topicKey, number, cancellationToken);
        }
        catch (InvalidDatabasePackageException ex)
        {
            error = ex.Message;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            error = $"Die Datenbank konnte nicht gelesen werden: {ex.Message}";
        }

        if (version != _loadVersion)
        {
            return;
        }

        if (details?.DatabaseName != Details?.DatabaseName)
        {
            ErDiagramUrl = null;
            ErDiagramError = null;
        }
        ShowErDiagram = false;
        Details = details;
        ErrorMessage = error;
        IsNotFound = details is null && error is null;
        IsLoading = false;
        NotifyStateChanged();
    }

    public void ClearQuery()
    {
        SqlQuery = string.Empty;
        ResultMessage = null;
        NotifyStateChanged();
    }

    public void ToggleHint()
    {
        ShowHint = !ShowHint;
        NotifyStateChanged();
    }

    public void ToggleSolution()
    {
        ShowSolution = !ShowSolution;
        NotifyStateChanged();
    }

    /// <summary>Opens the ER diagram dialog and loads the diagram on first use.</summary>
    public async Task OpenErDiagramAsync(CancellationToken cancellationToken = default)
    {
        if (Details is null)
        {
            return;
        }

        ShowErDiagram = true;
        NotifyStateChanged();
        if (ErDiagramUrl is not null)
        {
            return;
        }

        var databaseName = Details.DatabaseName;
        string? url = null;
        string? error = null;
        try
        {
            var bytes = await exercises.GetErDiagramAsync(databaseName, cancellationToken);
            if (bytes is null)
            {
                error = $"Die Datei \"{ExerciseService.ErDiagramFileName}\" fehlt.";
            }
            else
            {
                url = "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            error = $"Das ER-Diagramm konnte nicht gelesen werden: {ex.Message}";
        }

        if (Details?.DatabaseName != databaseName)
        {
            return;
        }

        ErDiagramUrl = url;
        ErDiagramError = error;
        NotifyStateChanged();
    }

    public void CloseErDiagram()
    {
        ShowErDiagram = false;
        NotifyStateChanged();
    }

    public void CheckQuery()
    {
        // Running and comparing the query is not connected yet.
        ResultMessage = string.IsNullOrWhiteSpace(SqlQuery)
            ? "Bitte gib zuerst eine SQL-Abfrage ein."
            : "Die Prüfung ist noch nicht angebunden.";
        NotifyStateChanged();
    }

    /// <summary>Relative URL of an exercise, e.g. <c>tasks/Kino%20(1)/select/1</c>.</summary>
    public static string GetExerciseUrl(string databaseName, ExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        return $"tasks/{Uri.EscapeDataString(databaseName)}/{TopicNames.GetKey(exercise.Topic).ToLowerInvariant()}/{exercise.Number}";
    }

    /// <summary>Relative URL of the task list of a database, e.g. <c>tasks/Kino%20(1)/</c>.</summary>
    public static string GetTaskListUrl(string databaseName) => $"tasks/{Uri.EscapeDataString(databaseName)}/";

    /// <summary>Formats a raw SQLite value for display; numbers use "." like in SQL.</summary>
    public static string FormatValue(object? value) => value switch
    {
        null => "NULL",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static int IndexInTopic(ExerciseDetails details) =>
        details.TopicExercises.ToList().FindIndex(e => e.Number == details.Exercise.Number);
}
