namespace SqlTrainer.UI.ViewModels;

/// <summary>
/// Base class for screen state. A view model holds the state of one screen, calls services
/// and raises <see cref="OnChange"/> after its state changed; the view then re-renders.
/// View models are registered as scoped, which in Blazor Server means one instance per browser tab.
/// </summary>
public abstract class ViewModelBase
{
    public event Action? OnChange;

    protected void NotifyStateChanged() => OnChange?.Invoke();
}
