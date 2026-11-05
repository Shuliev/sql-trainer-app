using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace SqlTrainer.Architecture.Tests;

/// <summary>
/// Dependencies point inwards only: App -> UI -> Application -> Core &lt;- Import/Execution/Storage.
/// Import, Execution and Storage don't know each other; App is the composition root and may use everything.
/// </summary>
public class LayerDependencyTests
{
    private static readonly Assembly AppAssembly = Assembly.Load("SqlTrainer.App");
    private static readonly Assembly UiAssembly = Assembly.Load("SqlTrainer.UI");
    private static readonly Assembly ApplicationAssembly = Assembly.Load("SqlTrainer.Application");
    private static readonly Assembly CoreAssembly = Assembly.Load("SqlTrainer.Core");
    private static readonly Assembly ImportAssembly = Assembly.Load("SqlTrainer.Import");
    private static readonly Assembly ExecutionAssembly = Assembly.Load("SqlTrainer.Execution");
    private static readonly Assembly StorageAssembly = Assembly.Load("SqlTrainer.Storage");

    private static readonly ArchUnitNET.Domain.Architecture Architecture = new ArchLoader()
        .LoadAssemblies(AppAssembly, UiAssembly, ApplicationAssembly, CoreAssembly, ImportAssembly, ExecutionAssembly, StorageAssembly)
        .Build();

    [Fact]
    public void Core_DependsOnNoOtherLayer() =>
        AssertNoDependency(CoreAssembly, AppAssembly, UiAssembly, ApplicationAssembly, ImportAssembly, ExecutionAssembly, StorageAssembly);

    [Fact]
    public void Import_DependsOnlyOnCore() =>
        AssertNoDependency(ImportAssembly, AppAssembly, UiAssembly, ApplicationAssembly, ExecutionAssembly, StorageAssembly);

    [Fact]
    public void Execution_DependsOnlyOnCore() =>
        AssertNoDependency(ExecutionAssembly, AppAssembly, UiAssembly, ApplicationAssembly, ImportAssembly, StorageAssembly);

    [Fact]
    public void Storage_DependsOnlyOnCore() =>
        AssertNoDependency(StorageAssembly, AppAssembly, UiAssembly, ApplicationAssembly, ImportAssembly, ExecutionAssembly);

    [Fact]
    public void Application_DependsOnlyOnCore() =>
        AssertNoDependency(ApplicationAssembly, AppAssembly, UiAssembly, ImportAssembly, ExecutionAssembly, StorageAssembly);

    [Fact]
    public void Ui_DependsNeitherOnHostNorOnInfrastructure() =>
        AssertNoDependency(UiAssembly, AppAssembly, ImportAssembly, ExecutionAssembly, StorageAssembly);

    private static void AssertNoDependency(Assembly layer, params Assembly[] forbidden)
    {
        IArchRule rule = Types().That().ResideInAssembly(layer).Should()
            .NotDependOnAny(Types().That().ResideInAssembly(forbidden[0], forbidden[1..]))
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }
}
