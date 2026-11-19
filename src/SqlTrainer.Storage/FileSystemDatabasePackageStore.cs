using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Storage;

/// <summary>
/// Stores each package in its own folder <c>&lt;Location&gt;/&lt;databaseName&gt;/</c>. Every user runs their
/// own copy of the app, so "the server" is the user's own device. If the preferred directory isn't
/// writable (e.g. the exe lies in C:\Program Files), the user's local application data folder is used.
/// </summary>
public sealed partial class FileSystemDatabasePackageStore : IDatabasePackageStore, IExerciseDatabaseLocator
{
    public const string FolderName = "Datenbanken";

    /// <summary>SQLite file that is created from <see cref="_scriptFileName"/> when a package is saved.</summary>
    public const string DatabaseFileName = "datenbank.db";

    private const string _scriptFileName = "datenbank.sql";

    private readonly ILogger<FileSystemDatabasePackageStore> _logger;

    public FileSystemDatabasePackageStore(string preferredDirectory, ILogger<FileSystemDatabasePackageStore> logger)
    {
        _logger = logger;
        Location = ResolveWritableDirectory(preferredDirectory);
    }

    public string Location { get; }

    public IReadOnlyList<string> ListNames() =>
        Directory.EnumerateDirectories(Location)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(name => !name.StartsWith('.'))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public bool Exists(string databaseName) =>
        IsValidName(databaseName) && Directory.Exists(Path.Combine(Location, databaseName));

    public async Task SaveAsync(string databaseName, DatabasePackage package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (!IsValidName(databaseName))
        {
            throw new InvalidDatabasePackageException("Ungültiger Datenbankname.");
        }

        // Build the folder under a hidden temporary name first, so a failed save leaves nothing half-written.
        var tempDir = Path.Combine(Location, $".import-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(tempDir);
            foreach (var (fileName, content) in package.Files)
            {
                if (!IsValidName(fileName))
                {
                    throw new InvalidDatabasePackageException($"Ungültiger Dateiname \"{fileName}\".");
                }
                await File.WriteAllBytesAsync(Path.Combine(tempDir, fileName), content, cancellationToken);
            }

            await CreateDatabaseAsync(tempDir, cancellationToken);

            var targetDir = Path.Combine(Location, databaseName);
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, recursive: true);
            }
            Directory.Move(tempDir, targetDir);
            LogSaved(databaseName, targetDir);
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    public string GetFilePath(string databaseName)
    {
        if (!IsValidName(databaseName))
        {
            throw new ArgumentException("Ungültiger Datenbankname.", nameof(databaseName));
        }

        return Path.Combine(Location, databaseName, DatabaseFileName);
    }

    /// <summary>
    /// Runs <c>datenbank.sql</c> in one transaction against a new <see cref="DatabaseFileName"/> in <paramref name="directory"/>.
    /// Pooling is off so the file is released as soon as the connection is disposed; otherwise it stays locked on
    /// Windows and the folder can't be moved.
    /// </summary>
    private static async Task CreateDatabaseAsync(string directory, CancellationToken cancellationToken)
    {
        var scriptPath = Path.Combine(directory, _scriptFileName);
        if (!File.Exists(scriptPath))
        {
            throw new InvalidDatabasePackageException($"Die Datei {_scriptFileName} fehlt.");
        }

        var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(directory, DatabaseFileName),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();

        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
#pragma warning disable CA2100 // The script is the customer's package content; it runs against a new, empty database file.
            command.CommandText = script;
#pragma warning restore CA2100
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (SqliteException ex)
        {
            throw new InvalidDatabasePackageException($"{_scriptFileName} konnte nicht ausgeführt werden: {ex.Message}", ex);
        }
    }

    /// <summary>A single, non-hidden path segment (no separators, no "." / "..").</summary>
    private static bool IsValidName(string name) =>
        !string.IsNullOrWhiteSpace(name)
        && !name.StartsWith('.')
        && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
        && name.IndexOfAny(['/', '\\']) < 0;

    private string ResolveWritableDirectory(string preferred)
    {
        if (IsWritable(preferred))
        {
            return preferred;
        }

        var fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SqlTrainer", FolderName);
        LogFallback(preferred, fallback);
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    private static bool IsWritable(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var probe = Path.Combine(directory, $".write-test-{Guid.NewGuid():N}");
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best effort: hidden leftovers are ignored by ListNames.
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Saved database \"{Name}\" to {Path}")]
    private partial void LogSaved(string name, string path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Preferred} is not writable, using {Fallback} instead")]
    private partial void LogFallback(string preferred, string fallback);
}
