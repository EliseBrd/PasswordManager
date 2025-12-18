using Microsoft.AspNetCore.Components;
using PasswordManager.Web.Components.Fragments;

namespace PasswordManager.Web.Components.Pages;

public partial class Home : ComponentBase
{
    private List<VaultList.VaultModel> vaults = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var accessibleVaults = await VaultService.GetAccessibleVaultsAsync();
            if (accessibleVaults != null)
            {
                vaults = accessibleVaults.Select(v => new VaultList.VaultModel
                {
                    Identifier = v.Identifier,
                    Name = v.Name,
                    IsShared = v.IsShared,
                    Category = v.IsShared ? "Partagé" : "Personnel"
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