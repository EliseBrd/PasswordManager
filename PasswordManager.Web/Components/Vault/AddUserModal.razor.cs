using Microsoft.AspNetCore.Components;
using PasswordManager.Dto.User;
using PasswordManager.Web.Services;

namespace PasswordManager.Web.Components.Vault
{
    public partial class AddUserModal : ComponentBase
    {
        [Inject] protected VaultService VaultService { get; set; } = default!;

        [Parameter] public string VaultId { get; set; } = "";
        [Parameter] public List<UserSummaryResponse> ExistingUsers { get; set; } = new();
        [Parameter] public bool IsShared { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<bool> OnSharingChanged { get; set; }

        protected string searchQuery = "";
        protected List<UserSummaryResponse> searchResults = new();
        protected bool isSearching = false;
        protected string errorMessage = "";

        protected async Task ToggleSharing(ChangeEventArgs e)
        {
            bool newValue = (bool)(e.Value ?? false);
            try
            {
                var success = await VaultService.UpdateVaultSharingAsync(VaultId, newValue);
                if (success)
                {
                    IsShared = newValue;
                    await OnSharingChanged.InvokeAsync(newValue);
                }
                else
                {
                    errorMessage = "Impossible de modifier le partage.";
                    // Revert UI if needed, but here we rely on re-render or user retry
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erreur lors de la modification du partage: {ex.Message}";
                Console.WriteLine($"Error updating sharing: {ex.Message}");
            }
        }

        protected async Task SearchUsers()
        {
            errorMessage = "";
            if (string.IsNullOrWhiteSpace(searchQuery) || searchQuery.Length < 3)
            {
                searchResults.Clear();
                return;
            }

            isSearching = true;
            try
            {
                var results = await VaultService.SearchUsersAsync(searchQuery);
                
                // Filter out existing users
                searchResults = results
                    .Where(u => !ExistingUsers.Any(eu => eu.Identifier == u.Identifier))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching users: {ex.Message}");
            }
            finally
            {
                isSearching = false;
            }
        }

        protected async Task AddUser(UserSummaryResponse user)
        {
            errorMessage = "";
            try
            {
                var success = await VaultService.AddUserToVaultAsync(VaultId, user.Identifier);
                if (success)
                {
                    searchQuery = "";
                    searchResults.Clear();
                    ExistingUsers.Add(user);
                    StateHasChanged();
                }
                else
                {
                    errorMessage = "Impossible d'ajouter l'utilisateur.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erreur lors de l'ajout: {ex.Message}";
                Console.WriteLine($"Error adding user: {ex.Message}");
            }
        }

        protected async Task RemoveUser(UserSummaryResponse user)
        {
            errorMessage = "";
            try
            {
                var success = await VaultService.RemoveUserFromVaultAsync(VaultId, user.Identifier);
                if (success)
                {
                    ExistingUsers.Remove(user);
                    StateHasChanged();
                }
                else
                {
                    errorMessage = "Impossible de supprimer l'utilisateur.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erreur lors de la suppression: {ex.Message}";
                Console.WriteLine($"Error removing user: {ex.Message}");
            }
        }
    }
}
