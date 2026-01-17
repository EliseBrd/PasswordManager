using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PasswordManager.Web.Services;

namespace PasswordManager.Web.Components.Fragments;

public partial class VaultEntry : ComponentBase
{
    [Inject] protected VaultEntryService VaultEntryService { get; set; } = default!;
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

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
    
    // Pour détecter les changements
    private string? _lastEncryptedData;

    protected override async Task OnParametersSetAsync()
    {
        // Si les données chiffrées ont changé, on force la mise à jour du DOM
        // Note : On ne peut pas appeler JS ici car le rendu n'est pas fini, on le fait dans OnAfterRenderAsync
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // On met à jour si c'est le premier rendu OU si les données ont changé
        if (!string.IsNullOrEmpty(EncryptedData) && (firstRender || EncryptedData != _lastEncryptedData))
        {
            _lastEncryptedData = EncryptedData;
            // Appel JS pour déchiffrer et remplir les champs
            await JS.InvokeVoidAsync("cryptoFunctions.decryptAndFillEntry", EncryptedData, TitleId, UsernameId);
        }
    }
    
    private async Task<string> GetCurrentUserEmail()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst("preferred_username")?.Value 
               ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
               ?? user.Identity?.Name 
               ?? "Unknown";
    }
    
    private async Task ShowPassword()
    {
        // Récupération de l'email pour le log
        var userEmail = await GetCurrentUserEmail();
        
        // Génération du log chiffré (récupère le titre dans le DOM)
        var encryptedLog = await JS.InvokeAsync<string>(
            "cryptoFunctions.encryptShowPasswordLog",
            Identifier.ToString(),
            userEmail
        );
        
        // 1. Récupérer le blob chiffré depuis l'API (via le service C#) en passant le log
        var encryptedPassword = await VaultEntryService.GetEntryPasswordAsync(Identifier, encryptedLog);

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