using Microsoft.AspNetCore.Components;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Web.Components.Fragments;

namespace PasswordManager.Web.Components.Pages;

public partial class Home : ComponentBase
{
    private List<VaultSummaryResponse> vaults = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var accessibleVaults = await VaultService.GetAccessibleVaultsAsync();
            if (accessibleVaults != null)
            {
                vaults = accessibleVaults.Select(v => new VaultSummaryResponse
                {
                    Identifier = v.Identifier,
                    Name = v.Name,
                    IsShared = v.IsShared,
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            // Handle error, e.g., show a message
            Console.WriteLine($"Error fetching vaults: {ex.Message}");
        }
    }
}