using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PasswordManager.Web.Services;

namespace PasswordManager.Web.Components.Fragments;

public partial class VaultEntry : ComponentBase
{
    [Parameter] public Guid Identifier { get; set; }
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Username { get; set; } = "";
    [Parameter] public string Password { get; set; } = "";
    [Parameter] public string Category { get; set; } = "Default";
    [Parameter] public EventCallback<Guid> OnDeleted { get; set; }
    [Parameter] public EventCallback OnShowPassword { get; set; }
    
    [Inject] public VaultEntryService VaultEntryService { get; set; } = default!;
    
    [Inject] IJSRuntime JS { get; set; } = default!;

    private bool showMenu = false;
    private async Task Copy(string text)
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
    
    private async Task OnDelete()
    {
        await VaultEntryService.DeleteVaultEntryAsync(Identifier);

        // Mise à jour UI locale
        await OnDeleted.InvokeAsync(Identifier);
    }


    private void ToggleMenu()
    {
        showMenu = !showMenu;
    }

    private string CategoryIcon =>
        Category?.ToLower() switch
        {
            "personnel" => "fa-solid fa-lock",
            "partagé" => "fa-solid fa-users",
            _ => "fa-solid fa-tag"
        };
}