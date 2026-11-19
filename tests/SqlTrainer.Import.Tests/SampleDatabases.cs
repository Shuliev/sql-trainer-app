namespace SqlTrainer.Import.Tests;

/// <summary>Finds the sample packages bundled in <c>src/SqlTrainer.App/Datenbanken</c>.</summary>
internal static class SampleDatabases
{
    public static IReadOnlyList<string> Files(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "sql-trainer-app.slnx")))
        {
            directory = directory.Parent;
        }
        Assert.NotNull(directory);

        var root = Path.Combine(directory.FullName, "src", "SqlTrainer.App", "Datenbanken");
        return Directory.EnumerateDirectories(root)
            .Select(folder => Path.Combine(folder, fileName))
            .Where(File.Exists)
            .ToList();
    }
}
