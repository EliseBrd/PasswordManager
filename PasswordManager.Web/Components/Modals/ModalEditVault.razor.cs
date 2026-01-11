using Microsoft.AspNetCore.Components;
using PasswordManager.Dto.Vault.Requests;

namespace PasswordManager.Web.Components.Modals;

public partial class ModalEditVault : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string InitialName { get; set; } = "";
    [Parameter] public EventCallback<UpdateVaultRequest> OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private string VaultName = "";
    private string? NewPassword;

    protected override void OnParametersSet()
    {
        VaultName = InitialName;
    }

    private async Task Confirm()
    {
        var request = new UpdateVaultRequest
        {
            Name = VaultName,
            NewPassword = string.IsNullOrWhiteSpace(NewPassword) ? null : NewPassword
        };

        await OnConfirm.InvokeAsync(request);
    }

    private async Task Cancel()
    {
        await OnCancel.InvokeAsync();
    }

    private void OnPasswordInput(ChangeEventArgs e)
    {
        NewPassword = e.Value?.ToString();
    }
    
    private Task OnPasswordGenerated(string pwd)
    {
        NewPassword = pwd;
        return Task.CompletedTask;
    }

}