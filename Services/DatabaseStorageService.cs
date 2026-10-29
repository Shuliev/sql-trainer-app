using System.IO.Compression;

namespace SqlApp.Services;

/// <summary>
/// Stores uploaded database packages (.zip) on the local machine. Since every user runs
/// their own copy of the app, "the server" is the user's own device.
/// Location: a "Datenbanken" folder next to SqlApp.exe (published) or in the project folder (development).
/// </summary>
public class DatabaseStorageService
{
    public const string FolderName = "Datenbanken";
    public const long MaxUploadBytes = 50 * 1024 * 1024; // 50 MB

    public static readonly string[] RequiredFiles =
        ["beschreibung.txt", "er_diagramm.png", "datenbank.sql", "aufgaben.txt"];

    private readonly ILogger<DatabaseStorageService> _logger;

    public string StorageDirectory { get; }

    public DatabaseStorageService(IWebHostEnvironment env, ILogger<DatabaseStorageService> logger)
    {
        _logger = logger;

        // AppContext.BaseDirectory is the folder containing the single-file exe. In development it
        // would be bin/Debug/..., which gets wiped on clean, so use the project folder instead.
        var baseDir = env.IsDevelopment() ? env.ContentRootPath : AppContext.BaseDirectory;
        StorageDirectory = ResolveWritableDirectory(Path.Combine(baseDir, FolderName));
    }

    public IReadOnlyList<string> ListDatabases() =>
        Directory.EnumerateDirectories(StorageDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(name => !name.StartsWith('.'))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public bool Exists(string databaseName) =>
        Directory.Exists(Path.Combine(StorageDirectory, databaseName));

    /// <summary>Derives a safe folder name from the uploaded zip's file name.</summary>
    public static string GetDatabaseName(string zipFileName)
    {
        var name = Path.GetFileNameWithoutExtension(zipFileName);
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Trim().Trim('.');
    }

    /// <summary>
    /// Validates the zip and extracts the required files into Datenbanken/&lt;databaseName&gt;/,
    /// replacing any existing database with the same name.
    /// The files may be at the zip root or inside a single top-level folder.
    /// Throws <see cref="InvalidDataException"/> with a user-facing message if the zip is invalid.
    /// </summary>
    public async Task SaveZipAsync(Stream zipStream, string databaseName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidDataException("Ungültiger Datenbankname.");
        }

        // The browser upload stream isn't seekable, which ZipArchive needs for reading.
        var tempZip = Path.Combine(StorageDirectory, $".upload-{Guid.NewGuid():N}.zip");
        var tempDir = Path.Combine(StorageDirectory, $".extract-{Guid.NewGuid():N}");
        try
        {
            await using (var fs = new FileStream(tempZip, FileMode.CreateNew))
            {
                await zipStream.CopyToAsync(fs, ct);
            }

            Directory.CreateDirectory(tempDir);
            ZipArchive archive;
            try
            {
                archive = ZipFile.OpenRead(tempZip);
            }
            catch (InvalidDataException)
            {
                throw new InvalidDataException("Die Datei ist keine gültige ZIP-Datei.");
            }

            using (archive)
            {
                var entries = archive.Entries
                    .Where(e => !string.IsNullOrEmpty(e.Name)
                                && !e.FullName.StartsWith("__MACOSX/", StringComparison.OrdinalIgnoreCase)
                                && e.FullName.Replace('\\', '/').Count(ch => ch == '/') <= 1)
                    .ToList();

                foreach (var required in RequiredFiles)
                {
                    var matches = entries
                        .Where(e => string.Equals(e.Name, required, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (matches.Count == 0)
                    {
                        throw new InvalidDataException($"Die Datei \"{required}\" fehlt in der ZIP-Datei.");
                    }
                    if (matches.Count > 1)
                    {
                        throw new InvalidDataException($"Die Datei \"{required}\" ist mehrfach in der ZIP-Datei enthalten.");
                    }

                    // The target path uses only the known file name, so malicious entry paths can't escape.
                    matches[0].ExtractToFile(Path.Combine(tempDir, required), overwrite: true);
                }
            }

            var targetDir = Path.Combine(StorageDirectory, databaseName);
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, recursive: true);
            }
            Directory.Move(tempDir, targetDir);
            _logger.LogInformation("Saved database \"{Name}\" to {Path}", databaseName, targetDir);
        }
        finally
        {
            try { File.Delete(tempZip); } catch { /* best effort */ }
            try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true); } catch { /* best effort */ }
        }
    }

    private string ResolveWritableDirectory(string preferred)
    {
        if (IsWritable(preferred))
        {
            return preferred;
        }

        // e.g. the exe was placed in a read-only location like C:\Program Files
        var fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SqlApp", FolderName);
        _logger.LogWarning("{Preferred} is not writable, using {Fallback} instead", preferred, fallback);
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    private static bool IsWritable(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            var probe = Path.Combine(dir, $".write-test-{Guid.NewGuid():N}");
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
