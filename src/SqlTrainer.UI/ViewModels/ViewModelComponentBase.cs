using Microsoft.AspNetCore.Components;

namespace SqlTrainer.UI.ViewModels;

/// <summary>Base for views bound to a view model: re-renders when the view model raises <see cref="ViewModelBase.OnChange"/>.</summary>
public abstract class ViewModelComponentBase<TViewModel> : ComponentBase, IDisposable
    where TViewModel : ViewModelBase
{
    [Inject]
    protected TViewModel ViewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        ViewModel.OnChange += HandleChange;
    }

    public void Dispose()
    {
        ViewModel.OnChange -= HandleChange;
        GC.SuppressFinalize(this);
    }

    private void HandleChange() => _ = InvokeAsync(StateHasChanged);
}
