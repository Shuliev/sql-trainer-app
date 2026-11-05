using SqlTrainer.UI.ViewModels;

namespace SqlTrainer.UI.Tests;

public class CounterViewModelTests
{
    [Fact]
    public void Increment_ChangesCountAndRaisesOnChange()
    {
        var viewModel = new CounterViewModel();
        var changes = 0;
        viewModel.OnChange += () => changes++;

        viewModel.Increment();

        Assert.Equal(2, viewModel.Count);
        Assert.Equal(1, changes);
    }
}
