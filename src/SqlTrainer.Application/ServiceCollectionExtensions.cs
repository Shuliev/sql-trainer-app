using Microsoft.Extensions.DependencyInjection;

namespace SqlTrainer.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlTrainerApplication(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseManagementService>();
        return services;
    }
}
