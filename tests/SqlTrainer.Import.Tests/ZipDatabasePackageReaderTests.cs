using System.IO.Compression;
using System.Text;
using SqlTrainer.Core;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Import.Tests;

public class ZipDatabasePackageReaderTests
{
    private readonly ZipDatabasePackageReader _reader = new();

    [Fact]
    public async Task ReadAsyncFilesAtRootReturnsAllRequiredFiles()
    {
        using var zip = CreateZip(DatabasePackageFormat.RequiredFiles.Select(name => (name, name)));

        var package = await _reader.ReadAsync(zip);

        Assert.Equal(DatabasePackageFormat.RequiredFiles.Order(), package.Files.Keys.Order());
        Assert.Equal("datenbank.sql", Encoding.UTF8.GetString(package.Files["datenbank.sql"]));
    }

    [Fact]
    public async Task ReadAsyncFilesInOneFolderWithOtherCaseAndBackslashesReturnsAllRequiredFiles()
    {
        using var zip = CreateZip(DatabasePackageFormat.RequiredFiles.Select(name => ($"Fuhrpark\\{name.ToUpperInvariant()}", name)));

        var package = await _reader.ReadAsync(zip);

        Assert.Equal(DatabasePackageFormat.RequiredFiles.Count, package.Files.Count);
        Assert.Equal("aufgaben.txt", Encoding.UTF8.GetString(package.Files["aufgaben.txt"]));
    }

    [Fact]
    public async Task ReadAsyncIgnoresMacOsMetadataAndDeeperFolders()
    {
        var entries = DatabasePackageFormat.RequiredFiles.Select(name => (name, name))
            .Append(("__MACOSX/beschreibung.txt", "x"))
            .Append(("a/b/beschreibung.txt", "x"));
        using var zip = CreateZip(entries);

        var package = await _reader.ReadAsync(zip);

        Assert.Equal("beschreibung.txt", Encoding.UTF8.GetString(package.Files["beschreibung.txt"]));
    }

    [Fact]
    public async Task ReadAsyncMissingFileThrows()
    {
        using var zip = CreateZip(DatabasePackageFormat.RequiredFiles.Skip(1).Select(name => (name, name)));

        var ex = await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _reader.ReadAsync(zip));

        Assert.Contains(DatabasePackageFormat.RequiredFiles[0], ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadAsyncDuplicateFileThrows()
    {
        var entries = DatabasePackageFormat.RequiredFiles.Select(name => (name, name)).Append(("x/DATENBANK.sql", "x"));
        using var zip = CreateZip(entries);

        var ex = await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _reader.ReadAsync(zip));

        Assert.Contains("mehrfach", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadAsyncNotAZipThrows()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("no zip"));

        await Assert.ThrowsAsync<InvalidDatabasePackageException>(() => _reader.ReadAsync(stream));
    }

    private static MemoryStream CreateZip(IEnumerable<(string Path, string Content)> entries)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, content) in entries)
            {
                using var writer = new StreamWriter(archive.CreateEntry(path).Open());
                writer.Write(content);
            }
        }
        stream.Position = 0;
        return stream;
    }
}
