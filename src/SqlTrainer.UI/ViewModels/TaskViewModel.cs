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

    public string TopicDisplayName => Details is null ? string.Empty : TopicNames.GetDisplayName(Details.Exercise.Topic);

    /// <summary>1-based position of the exercise within its topic.</summary>
    public int Position => Details is null ? 0 : IndexInTopic(Details) + 1;

    public int TopicExerciseCount => Details?.TopicExercises.Count ?? 0;

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
