using Microsoft.Extensions.DependencyInjection;
using SqlTrainer.Core.Abstractions;

namespace SqlTrainer.Import;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlTrainerImport(this IServiceCollection services)
    {
        services.AddSingleton<IDatabasePackageReader, ZipDatabasePackageReader>();
        return services;
    }
}
