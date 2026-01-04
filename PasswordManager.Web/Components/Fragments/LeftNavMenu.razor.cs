using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using PasswordManager.Web.Services;
using Microsoft.Identity.Web;

namespace PasswordManager.Web.Components.Fragments;

public partial class LeftNavMenu : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private VaultStateService VaultState { get; set; } = default!;
    [Inject] private VaultService VaultService { get; set; } = default!;
    [Inject] protected MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; } = default!;
    
    private int TotalCount { get; set; }
    private int PersonalCount { get; set; }
    private int SharedCount { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadCountsAsync();
    }

    private async Task LoadCountsAsync()
    {
        try
        {
            // On récupère tous les vaults pour calculer les compteurs
            var allVaults = await VaultService.GetAccessibleVaultsAsync(null);
            if (allVaults != null)
            {
                var vaults = allVaults.ToList();
                TotalCount = vaults.Count;
                PersonalCount = vaults.Count(v => !v.IsShared);
                SharedCount = vaults.Count(v => v.IsShared);
            }
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            ConsentHandler.HandleException(ex);
        }
        catch (Exception)
        {
            // Ignorer les erreurs silencieusement pour le menu
        }
    }

    void GoToCreateVault()
    {
        NavManager.NavigateTo("/createVault");
    }
    
    private void FilterVaults(bool? isShared)
    {
        VaultState.SetFilter(isShared);
        
        // Si on n'est pas sur la page d'accueil, on redirige
        if (NavManager.Uri != NavManager.BaseUri)
        {
            NavManager.NavigateTo("/");
        }
    }
    
    string GetDisplayName(ClaimsPrincipal user)
    {
        if (user == null) return "Utilisateur";

        // Cherche le claim "name"
        var nameClaim = user.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        if (!string.IsNullOrWhiteSpace(nameClaim))
            return nameClaim;

        // fallback
        return user.Identity?.Name ?? "Utilisateur";
    }

    string GetEmail(ClaimsPrincipal user)
        => user.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
           ?? user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
           ?? "";

    string GetInitials(ClaimsPrincipal user)
    {
        var name = user.Identity?.Name;
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0][0].ToString().ToUpper();

        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}