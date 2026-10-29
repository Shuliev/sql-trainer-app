namespace SqlTrainer.UI.ViewModels;

/// <summary>Template demo screen; shows the view model pattern until the real screens exist.</summary>
public sealed class CounterViewModel : ViewModelBase
{
    public int Count { get; private set; }

    public void Increment()
    {
        Count += 2;
        NotifyStateChanged();
    }
}
