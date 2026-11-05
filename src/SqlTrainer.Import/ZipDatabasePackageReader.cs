using System.IO.Compression;
using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Import;

/// <summary>
/// Reads the required files straight from the ZIP into memory; nothing is extracted to disk, so entry
/// names from the ZIP never become file paths. The files may be at the ZIP root or inside one
/// top-level folder; file names are matched case-insensitively.
/// </summary>
public sealed class ZipDatabasePackageReader : IDatabasePackageReader
{
    private const string _macOsMetadataFolder = "__MACOSX/";

    public async Task<DatabasePackage> ReadAsync(Stream zipStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zipStream);

        // ZipArchive needs a seekable stream; browser upload streams aren't.
        using var buffer = new MemoryStream();
        await CopyWithLimitAsync(zipStream, buffer, cancellationToken);
        buffer.Position = 0;

        ZipArchive archive;
        try
        {
            archive = new ZipArchive(buffer, ZipArchiveMode.Read);
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidDatabasePackageException("Die Datei ist keine gültige ZIP-Datei.", ex);
        }

        using (archive)
        {
            var entries = archive.Entries
                .Select(entry => (Entry: entry, Path: entry.FullName.Replace('\\', '/')))
                .Where(e => !e.Path.EndsWith('/')
                            && !e.Path.StartsWith(_macOsMetadataFolder, StringComparison.OrdinalIgnoreCase)
                            && e.Path.Count(ch => ch == '/') <= 1)
                .Select(e => (e.Entry, Name: e.Path[(e.Path.LastIndexOf('/') + 1)..]))
                .ToList();

            var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var required in DatabasePackageFormat.RequiredFiles)
            {
                var matches = entries
                    .Where(e => string.Equals(e.Name, required, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matches.Count == 0)
                {
                    throw new InvalidDatabasePackageException($"Die Datei \"{required}\" fehlt in der ZIP-Datei.");
                }
                if (matches.Count > 1)
                {
                    throw new InvalidDatabasePackageException($"Die Datei \"{required}\" ist mehrfach in der ZIP-Datei enthalten.");
                }

                files[required] = await ReadEntryAsync(matches[0].Entry, cancellationToken);
            }

            if (files.Values.Sum(content => (long)content.Length) > DatabasePackageFormat.MaxPackageBytes)
            {
                throw TooLarge();
            }

            return new DatabasePackage(files);
        }
    }

    private static async Task<byte[]> ReadEntryAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            await using var source = await entry.OpenAsync(cancellationToken);
            using var target = new MemoryStream();
            await CopyWithLimitAsync(source, target, cancellationToken);
            return target.ToArray();
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidDatabasePackageException($"Die Datei \"{entry.FullName}\" in der ZIP-Datei ist beschädigt.", ex);
        }
    }

    /// <summary>Copies at most <see cref="DatabasePackageFormat.MaxPackageBytes"/>; the sizes in the ZIP header can't be trusted.</summary>
    private static async Task CopyWithLimitAsync(Stream source, Stream target, CancellationToken cancellationToken)
    {
        var chunk = new byte[81920];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(chunk, cancellationToken)) > 0)
        {
            total += read;
            if (total > DatabasePackageFormat.MaxPackageBytes)
            {
                throw TooLarge();
            }
            await target.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
    }

    private static InvalidDatabasePackageException TooLarge() =>
        new($"Die Datei ist zu groß (max. {DatabasePackageFormat.MaxPackageBytes / 1024 / 1024} MB).");
}
