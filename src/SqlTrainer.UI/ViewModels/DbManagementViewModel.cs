using SqlTrainer.Application;
using SqlTrainer.Core;
using SqlTrainer.Core.Models;

namespace SqlTrainer.UI.ViewModels;

/// <summary>State of the "Datenbanken verwalten" screen: stored databases and the ZIP upload.</summary>
public sealed class DbManagementViewModel(DatabaseManagementService databases) : ViewModelBase
{
    public static IReadOnlyList<string> RequiredFiles => DatabasePackageFormat.RequiredFiles;

    public static long MaxUploadBytes => DatabasePackageFormat.MaxPackageBytes;

    public IReadOnlyList<string> Databases { get; private set; } = [];

    public bool IsUploading { get; private set; }

    public string StorageLocation => databases.StorageLocation;

    public void Load()
    {
        Databases = databases.ListDatabases();
        NotifyStateChanged();
    }

    /// <summary>Checks the selected file before uploading; returns an error message or <c>null</c>.</summary>
    public static string? ValidateFile(string fileName, long size)
    {
        if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return "Bitte eine ZIP-Datei auswählen.";
        }
        if (size > MaxUploadBytes)
        {
            return $"Die Datei ist zu groß (max. {MaxUploadBytes / 1024 / 1024} MB).";
        }
        return null;
    }

    public static string GetDatabaseName(string fileName) => DatabasePackageFormat.GetDatabaseName(fileName);

    public bool Exists(string databaseName) => databases.Exists(databaseName);

    public async Task<UploadResult> UploadAsync(Stream zipStream, string databaseName, CancellationToken cancellationToken = default)
    {
        IsUploading = true;
        NotifyStateChanged();
        try
        {
            await databases.ImportAsync(zipStream, databaseName, cancellationToken);
            Databases = databases.ListDatabases();
            return new UploadResult(true, $"Datenbank \"{databaseName}\" wurde gespeichert.");
        }
        catch (InvalidDatabasePackageException ex)
        {
            return new UploadResult(false, ex.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new UploadResult(false, $"Fehler beim Hochladen: {ex.Message}");
        }
        finally
        {
            IsUploading = false;
            NotifyStateChanged();
        }
    }
}

public sealed record UploadResult(bool Succeeded, string Message);
