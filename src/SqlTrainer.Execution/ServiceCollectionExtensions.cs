using Microsoft.Extensions.DependencyInjection;
using SqlTrainer.Core.Abstractions;

namespace SqlTrainer.Execution;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlTrainerExecution(this IServiceCollection services)
    {
        services.AddSingleton<IQueryExecutor, SqliteQueryExecutor>();
        return services;
    }
}
