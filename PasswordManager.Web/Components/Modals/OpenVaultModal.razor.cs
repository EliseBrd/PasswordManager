using Microsoft.AspNetCore.Components;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Web.Services;
using Microsoft.JSInterop;
using System.Text.Json;

namespace PasswordManager.Web.Components.Modals;

public partial class OpenVaultModal : ComponentBase
{
    [Parameter]
    public string MasterPassword { get; set; } = "";

    [Parameter]
    public EventCallback<string> MasterPasswordChanged { get; set; }

    [Parameter, EditorRequired]
    public string VaultName { get; set; } = "";

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnUnlock { get; set; }

    private async Task OnPasswordChanged(ChangeEventArgs e)
    {
        await MasterPasswordChanged.InvokeAsync(e.Value?.ToString() ?? "");
    }
}