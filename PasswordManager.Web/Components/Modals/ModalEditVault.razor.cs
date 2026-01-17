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
    private string? ConfirmNewPassword;
    private string ErrorMessage = "";

    protected override void OnParametersSet()
    {
        VaultName = InitialName;
        // Reset des champs à l'ouverture
        NewPassword = null;
        ConfirmNewPassword = null;
        ErrorMessage = "";
    }

    private async Task Confirm()
    {
        ErrorMessage = "";

        // Validation du mot de passe si renseigné
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            if (NewPassword != ConfirmNewPassword)
            {
                ErrorMessage = "Les mots de passe ne correspondent pas.";
                return;
            }
            
            if (NewPassword.Length < 8)
            {
                ErrorMessage = "Le mot de passe doit faire au moins 8 caractères.";
                return;
            }
        }

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
    
    private void OnConfirmPasswordInput(ChangeEventArgs e)
    {
        ConfirmNewPassword = e.Value?.ToString();
    }
    
    private Task OnPasswordGenerated(string pwd)
    {
        NewPassword = pwd;
        ConfirmNewPassword = pwd; // On remplit aussi la confirmation pour simplifier la vie de l'user
        return Task.CompletedTask;
    }

}
