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
    [Parameter] public EventCallback OnShowPassword { get; set; }
    [Parameter] public EventCallback<Guid> OnAskDelete { get; set; }
    [Parameter] public EventCallback<Guid> OnAskEdit { get; set; }
    
    [Inject] IJSRuntime JS { get; set; } = default!;
    
    private async Task AskDelete()
    {
        await OnAskDelete.InvokeAsync(Identifier);
    }
    
    private async Task AskEdit()
    {
        await OnAskEdit.InvokeAsync(Identifier);
    }

    private string CategoryIcon => Category?.ToLower() switch
    {
        "personnel" => "fa-solid fa-lock",
        "partagé" => "fa-solid fa-users",
        _ => "fa-solid fa-tag"
    };
}