using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace PasswordManager.Web.Components.Fragments;

public partial class LeftNavMenu : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    void GoToCreateVault()
    {
        NavManager.NavigateTo("/createVault");
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