using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Web;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Web.Services;

namespace PasswordManager.Web.Components.Pages;

public partial class Home : ComponentBase, IDisposable
{
    [Inject] protected VaultService VaultService { get; set; } = default!;
    [Inject] protected VaultStateService VaultState { get; set; } = default!;
    [Inject] protected MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; } = default!;

    private List<VaultSummaryResponse> vaults = new();
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        VaultState.OnFilterChanged += HandleFilterChanged;
        await LoadVaultsAsync();
    }

    private async void HandleFilterChanged()
    {
        await LoadVaultsAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadVaultsAsync()
    {
        IsLoading = true;
        try
        {
            var accessibleVaults = await VaultService.GetAccessibleVaultsAsync(VaultState.CurrentFilter);
            if (accessibleVaults != null)
            {
                vaults = accessibleVaults.ToList();
            }
            else
            {
                vaults = new List<VaultSummaryResponse>();
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

    public void Dispose()
    {
        VaultState.OnFilterChanged -= HandleFilterChanged;
    }
}
