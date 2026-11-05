using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlTrainer.Core.Abstractions;

namespace SqlTrainer.Storage;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers the file-based store; <paramref name="databaseDirectory"/> is the preferred folder (falls back to local app data if not writable).</summary>
    public static IServiceCollection AddSqlTrainerStorage(this IServiceCollection services, string databaseDirectory)
    {
        services.AddSingleton<IDatabasePackageStore>(provider => new FileSystemDatabasePackageStore(
            databaseDirectory, provider.GetRequiredService<ILogger<FileSystemDatabasePackageStore>>()));
        return services;
    }
}
