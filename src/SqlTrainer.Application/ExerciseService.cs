using System.Text;
using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Application;

/// <summary>Use cases of the exercise screen: load one exercise of a stored database together with its tables.</summary>
public sealed class ExerciseService(IDatabasePackageStore store, IExerciseFileParser parser, IDatabaseInspector inspector)
{
    public const string ExerciseFileName = "aufgaben.txt";
    public const string SetupScriptFileName = "datenbank.sql";
    public const string ErDiagramFileName = "er_diagramm.png";

    /// <summary>Number of sample rows shown per table.</summary>
    public const int SampleRowLimit = 5;

    private static readonly UTF8Encoding _strictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>
    /// Loads exercise <paramref name="number"/> (the Nr from <c>aufgaben.txt</c>) of <paramref name="topicKey"/>
    /// (e.g. "select"); <c>null</c> if the database, topic or exercise doesn't exist.
    /// </summary>
    /// <exception cref="InvalidDatabasePackageException">A stored file is broken; the message is meant for the user.</exception>
    public async Task<ExerciseDetails?> GetExerciseAsync(string databaseName, string topicKey, int number, CancellationToken cancellationToken = default)
    {
        if (!TopicNames.TryParse(topicKey, out var topic) || !store.Exists(databaseName))
        {
            return null;
        }

        var exercises = parser.Parse(await ReadTextAsync(databaseName, ExerciseFileName, cancellationToken));
        var topicExercises = exercises.Where(e => e.Topic == topic).OrderBy(e => e.Number).ToList();
        var exercise = topicExercises.Find(e => e.Number == number);
        if (exercise is null)
        {
            return null;
        }

        var setupScript = await ReadTextAsync(databaseName, SetupScriptFileName, cancellationToken);
        var tables = await inspector.InspectAsync(setupScript, SampleRowLimit, cancellationToken);
        return new ExerciseDetails(databaseName, exercise, topicExercises, tables);
    }

    /// <summary>PNG bytes of the ER diagram of <paramref name="databaseName"/>; <c>null</c> if the database or the file doesn't exist.</summary>
    public Task<byte[]?> GetErDiagramAsync(string databaseName, CancellationToken cancellationToken = default) =>
        store.ReadFileAsync(databaseName, ErDiagramFileName, cancellationToken);

    private async Task<string> ReadTextAsync(string databaseName, string fileName, CancellationToken cancellationToken)
    {
        var bytes = await store.ReadFileAsync(databaseName, fileName, cancellationToken)
            ?? throw new InvalidDatabasePackageException($"Die Datei \"{fileName}\" fehlt.");
        try
        {
            return _strictUtf8.GetString(bytes).TrimStart('\uFEFF');
        }
        catch (DecoderFallbackException ex)
        {
            throw new InvalidDatabasePackageException($"Die Datei \"{fileName}\" ist nicht UTF-8-kodiert.", ex);
        }
    }
}

/// <summary>One exercise with everything the exercise screen shows.</summary>
/// <param name="DatabaseName">Name of the stored database (its folder name).</param>
/// <param name="Exercise">The requested exercise.</param>
/// <param name="TopicExercises">All exercises of the same topic, ordered by number (includes <paramref name="Exercise"/>).</param>
/// <param name="Tables">Tables of the practice database with sample rows.</param>
public sealed record ExerciseDetails(
    string DatabaseName,
    ExerciseDefinition Exercise,
    IReadOnlyList<ExerciseDefinition> TopicExercises,
    IReadOnlyList<DatabaseTable> Tables);
