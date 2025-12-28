using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Web.Services;
using System.Text.Json;

namespace PasswordManager.Web.Components.Pages
{
    public partial class VaultDetails : ComponentBase, IDisposable
    {
        [Inject] protected VaultService VaultService { get; set; } = default!;
        [Inject] protected VaultEntryService VaultEntryService { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] protected ILogger<VaultDetails> Logger { get; set; } = default!;

        [Parameter]
        public string VaultId { get; set; } = "";

        protected VaultDetailsResponse? vault;
        protected VaultUnlockResponse? unlockedVaultData;
        protected string masterPassword = "";
        protected bool isUnlocked = false;
        protected string errorMessage = "";
        
        protected List<DecryptedVaultEntry> decryptedEntries = new();
        
        protected bool showCreateForm = false;
        protected bool showShareModal = false;
        protected DecryptedVaultEntry newEntry = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                vault = await VaultService.GetVaultDetailsAsync(VaultId);
            }
            catch (Exception ex)
            {
                errorMessage = "Impossible de charger les détails du coffre.";
                Logger.LogError(ex, "Error loading vault details");
            }
        }

        protected async Task UnlockVault()
        {
            if (vault == null || string.IsNullOrWhiteSpace(masterPassword))
            {
                errorMessage = "Veuillez entrer le mot de passe.";
                return;
            }

            try
            {
                unlockedVaultData = await VaultService.UnlockVaultAsync(VaultId, masterPassword);

                if (unlockedVaultData == null)
                {
                    errorMessage = "Mot de passe incorrect.";
                    return;
                }

                await JSRuntime.InvokeVoidAsync("cryptoFunctions.deriveKeyAndDecrypt", masterPassword, unlockedVaultData.MasterSalt, unlockedVaultData.EncryptedKey);
                
                masterPassword = "";

                decryptedEntries.Clear();
                foreach (var entryDto in unlockedVaultData.Entries)
                {
                    var decryptedData = await JSRuntime.InvokeAsync<string>("cryptoFunctions.decryptData", entryDto.EncryptedData);
                    var entryDetails = JsonSerializer.Deserialize<DecryptedVaultEntry>(decryptedData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (entryDetails != null)
                    {
                        entryDetails.Identifier = Guid.Parse(entryDto.Identifier);
                        decryptedEntries.Add(entryDetails);
                    }
                }
                isUnlocked = true;
                errorMessage = "";
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = "Erreur lors du déchiffrement.";
                Logger.LogError(ex, "Error unlocking vault");
            }
        }

        protected async Task CreateEntry()
        {
            if (string.IsNullOrWhiteSpace(newEntry.Title)) return;

            var dataToEncrypt = new { newEntry.Title, newEntry.Username };
            var jsonData = JsonSerializer.Serialize(dataToEncrypt);
            
            var encryptedData = await JSRuntime.InvokeAsync<string>("cryptoFunctions.encryptData", jsonData);
            var encryptedPassword = await JSRuntime.InvokeAsync<string>("cryptoFunctions.encryptData", newEntry.Password);

            var request = new CreateVaultEntryRequest
            {
                VaultIdentifier = VaultId,
                EncryptedData = encryptedData,
                EncryptedPassword = encryptedPassword
            };

            await VaultEntryService.CreateVaultEntryAsync(request);

            decryptedEntries.Add(newEntry);
            newEntry = new();
            showCreateForm = false;
        }

        protected async Task ShowPassword(DecryptedVaultEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.Password))
                return;

            var encryptedPassword =
                await VaultEntryService.GetVaultEntryPasswordAsync(entry.Identifier);

            if (encryptedPassword != null)
            {
                entry.Password =
                    await JSRuntime.InvokeAsync<string>("cryptoFunctions.decryptData", encryptedPassword);

                StateHasChanged();
            }
        }

        protected void DeleteEntry(Guid identifier)
        {
            decryptedEntries.RemoveAll(e => e.Identifier == identifier);
        }


        protected void ToggleShare()
        {
            showShareModal = !showShareModal;
        }

        protected void OnSharingChanged(bool isShared)
        {
            if (vault != null)
            {
                vault.IsShared = isShared;
            }
        }

        public void Dispose()
        {
            _ = JSRuntime.InvokeVoidAsync("cryptoFunctions.clearKey");
        }

        public class DecryptedVaultEntry
        {
            public Guid Identifier { get; set; }
            public string Title { get; set; } = "";
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }
        
        private void GoBack()
        {
            Navigation.NavigateTo("/");
        }

    }
}
