using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Web;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Web.Services;

namespace PasswordManager.Web.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] protected VaultService VaultService { get; set; } = default!;
    [Inject] protected MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; } = default!;

    private List<VaultSummaryResponse> vaults = new();
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var accessibleVaults = await VaultService.GetAccessibleVaultsAsync();
            if (accessibleVaults != null)
            {
                vaults = accessibleVaults.ToList();
            }
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            ConsentHandler.HandleException(ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching vaults: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
