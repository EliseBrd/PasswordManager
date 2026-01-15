using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PasswordManager.Web.Services;

namespace PasswordManager.Web.Components.Fragments;

public partial class VaultEntry : ComponentBase
{
    [Inject] protected VaultEntryService VaultEntryService { get; set; } = default!;
    [Inject] IJSRuntime JS { get; set; } = default!;

    [Parameter] public Guid Identifier { get; set; }
    
    // Zero-Knowledge : On reçoit les données chiffrées
    [Parameter] public string EncryptedData { get; set; } = "";
    
    [Parameter] public string Category { get; set; } = "Default";
    [Parameter] public EventCallback<Guid> OnAskDelete { get; set; }
    [Parameter] public EventCallback<Guid> OnAskEdit { get; set; }
    
    // IDs uniques pour le DOM
    private string TitleId => $"title-{Identifier}";
    private string UsernameId => $"username-{Identifier}";
    private string PasswordId => $"password-{Identifier}";
    
    private string? _lastEncryptedData;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!string.IsNullOrEmpty(EncryptedData) &&
            EncryptedData != _lastEncryptedData)
        {
            _lastEncryptedData = EncryptedData;

            // Appel JS pour déchiffrer et remplir les champs
            await JS.InvokeVoidAsync("cryptoFunctions.decryptAndFillEntry", EncryptedData, TitleId, UsernameId);
        }
    }

    
    private async Task ShowPassword()
    {
        // 1. Récupérer le blob chiffré depuis l'API (via le service C#)
        // Le serveur voit le blob chiffré, c'est OK.
        var encryptedPassword = await VaultEntryService.GetEntryPasswordAsync(Identifier);

        if (encryptedPassword != null)
        {
            // 2. Envoyer le blob chiffré au JS pour déchiffrement et affichage
            // Le mot de passe en clair n'est jamais stocké en C#
            await JS.InvokeVoidAsync("cryptoFunctions.decryptAndShowPassword", encryptedPassword, PasswordId);
        }
    }
    
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