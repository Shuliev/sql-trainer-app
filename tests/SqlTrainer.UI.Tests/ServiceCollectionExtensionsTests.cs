using Microsoft.Extensions.DependencyInjection;
using SqlTrainer.UI.ViewModels;

namespace SqlTrainer.UI.Tests;

public class ServiceCollectionExtensionsTests
{
    private static readonly Type[] ViewModelTypes = typeof(ViewModelBase).Assembly.GetTypes()
        .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsSubclassOf(typeof(ViewModelBase)))
        .ToArray();

    [Fact]
    public void AddSqlTrainerUi_RegistersEveryViewModelAsScoped()
    {
        var services = new ServiceCollection().AddSqlTrainerUi();

        Assert.NotEmpty(ViewModelTypes);
        Assert.All(ViewModelTypes, type =>
        {
            var descriptor = Assert.Single(services, service => service.ServiceType == type);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        });
    }

    [Fact]
    public void AddSqlTrainerUi_GivesEachScopeItsOwnViewModel()
    {
        using var provider = new ServiceCollection().AddSqlTrainerUi().BuildServiceProvider(validateScopes: true);
        using var firstTab = provider.CreateScope();
        using var secondTab = provider.CreateScope();

        var first = firstTab.ServiceProvider.GetRequiredService<CounterViewModel>();

        Assert.Same(first, firstTab.ServiceProvider.GetRequiredService<CounterViewModel>());
        Assert.NotSame(first, secondTab.ServiceProvider.GetRequiredService<CounterViewModel>());
    }
}
