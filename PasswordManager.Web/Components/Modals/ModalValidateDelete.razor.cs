using Microsoft.AspNetCore.Components;

namespace PasswordManager.Web.Components.Modals;

public partial class ModalValidateDelete : ComponentBase
{
    [Parameter] public string Title { get; set; } = "Confirmation";
    [Parameter] public string Message { get; set; } = "Êtes-vous sûr ?";
    [Parameter] public EventCallback OnConfirmed { get; set; }

    private bool IsVisible { get; set; }

    public void Show(string message = null, string title = null)
    {
        if (message != null) Message = message;
        if (title != null) Title = title;
        IsVisible = true;
        StateHasChanged();
    }

    private void Cancel()
    {
        IsVisible = false;
    }

    private async Task Confirm()
    {
        IsVisible = false;
        if (OnConfirmed.HasDelegate)
            await OnConfirmed.InvokeAsync();
    }
}