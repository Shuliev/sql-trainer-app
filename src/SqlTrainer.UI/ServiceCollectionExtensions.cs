using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using SqlTrainer.UI.ViewModels;

namespace SqlTrainer.UI;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers MudBlazor and every view model of this assembly as scoped (one per browser tab).</summary>
    public static IServiceCollection AddSqlTrainerUi(this IServiceCollection services)
    {
        services.AddMudServices();

        var viewModelTypes = typeof(ViewModelBase).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsSubclassOf(typeof(ViewModelBase)));
        foreach (var type in viewModelTypes)
        {
            services.AddScoped(type);
        }

        return services;
    }
}
