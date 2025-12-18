using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;

namespace PasswordManager.Web.Components.Pages;

public partial class VaultDetails : ComponentBase
{
    [Parameter]
    public string VaultId { get; set; } = "";

    private VaultDetailsResponse? vault;
    private string masterPassword = "";
    private bool isUnlocked = false;
    private string errorMessage = "";
    private string decryptedVaultKey = ""; // Store the decrypted key here

    private List<DecryptedVaultEntry> decryptedEntries = new();
    
    private bool showCreateForm = false;
    private DecryptedVaultEntry newEntry = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            vault = await VaultService.GetVaultDetailsAsync(VaultId);
        }
        catch (Exception ex)
        {
            errorMessage = "Impossible de charger les détails du coffre.";
            Logger.LogError(ex, "Error in OnInitializedAsync");
        }
    }

    private async Task UnlockVault()
    {
        if (vault == null || string.IsNullOrWhiteSpace(masterPassword))
        {
            errorMessage = "Veuillez entrer le mot de passe.";
            return;
        }

        try
        {
            decryptedVaultKey = await JSRuntime.InvokeAsync<string>("cryptoFunctions.deriveKeyAndDecrypt", masterPassword, vault.MasterSalt, vault.EncryptedKey);

            foreach (var entryDto in vault.Entries)
            {
                var decryptedData = await JSRuntime.InvokeAsync<string>("cryptoFunctions.decryptData", decryptedVaultKey, entryDto.EncryptedData);
                var entryDetails = JsonSerializer.Deserialize<DecryptedVaultEntry>(decryptedData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (entryDetails != null)
                {
                    entryDetails.Id = int.Parse(entryDto.Identifier); // Keep track of the real ID
                    decryptedEntries.Add(entryDetails);
                }
            }
            isUnlocked = true;
            errorMessage = "";
        }
        catch (Exception)
        {
            errorMessage = "Mot de passe incorrect ou erreur de déchiffrement.";
        }
    }

    private async Task CreateEntry()
    {
        if (string.IsNullOrWhiteSpace(newEntry.Title)) return;

        // Create a version of the entry without the password for EncryptedData
        var dataToEncrypt = new { newEntry.Title, newEntry.Username };
        var jsonData = JsonSerializer.Serialize(dataToEncrypt);
        var encryptedData = await JSRuntime.InvokeAsync<string>("cryptoFunctions.encryptData", decryptedVaultKey, jsonData);

        // Encrypt the password separately
        var encryptedPassword = await JSRuntime.InvokeAsync<string>("cryptoFunctions.encryptData", decryptedVaultKey, newEntry.Password);

        var request = new CreateVaultEntryRequest
        {
            VaultIdentifier = VaultId,
            EncryptedData = encryptedData,
            EncryptedPassword = encryptedPassword
        };

        await VaultService.CreateVaultEntryAsync(request);

        // Refresh UI (simplified)
        decryptedEntries.Add(newEntry);
        newEntry = new();
        showCreateForm = false;
    }

    private async Task ShowPassword(DecryptedVaultEntry entry)
    {
        // If password is not already revealed
        if (string.IsNullOrEmpty(entry.Password))
        {
            var encryptedPassword = await VaultService.GetVaultEntryPasswordAsync(entry.Id);
            if (encryptedPassword != null)
            {
                entry.Password = await JSRuntime.InvokeAsync<string>("cryptoFunctions.decryptData", decryptedVaultKey, encryptedPassword);
                StateHasChanged(); // Refresh the UI to show the password
            }
        }
    }

    private async Task ToggleShare()
    {
        if (vault == null) return;

        try
        {
            await VaultService.ShareVaultAsync(VaultId);
            vault.IsShared = !vault.IsShared; // Toggle the state visually
            StateHasChanged();
        }
        catch (Exception)
        {
            // Handle error
        }
    }

    private void DeleteEntry(int id)
    {
        // To be implemented
    }

    public class DecryptedVaultEntry
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = ""; // Will be filled on demand
    }
}