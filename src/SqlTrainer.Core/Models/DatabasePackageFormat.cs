namespace SqlTrainer.Core.Models;

/// <summary>Rules of the import package format (see docs/Konzept Architektur/Draft Importformat.md).</summary>
public static class DatabasePackageFormat
{
    /// <summary>Upper limit for the uploaded ZIP and for its unpacked contents.</summary>
    public const long MaxPackageBytes = 50 * 1024 * 1024;

    public static IReadOnlyList<string> RequiredFiles { get; } =
        ["beschreibung.txt", "er_diagramm.png", "datenbank.sql", "aufgaben.txt"];

    /// <summary>Derives the database name from the uploaded file name, e.g. "Name_der_Datenbank.zip" → "Name_der_Datenbank".</summary>
    public static string GetDatabaseName(string zipFileName)
    {
        ArgumentNullException.ThrowIfNull(zipFileName);

        var name = Path.GetFileNameWithoutExtension(zipFileName);
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Trim().Trim('.');
    }
}
