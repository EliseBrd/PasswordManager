using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Web.Services;
using System.Text.Json;
using PasswordManager.Dto.VaultsEntries.Requests;
using PasswordManager.Web.Components.Modals;

namespace PasswordManager.Web.Components.Pages
{
    public partial class VaultDetails : ComponentBase, IDisposable
    {
        [Inject] protected VaultService VaultService { get; set; } = default!;
        [Inject] protected VaultEntryService VaultEntryService { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] protected ILogger<VaultDetails> Logger { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        [Parameter]
        public string VaultId { get; set; } = ""; // Changed to string to avoid casting error

        protected Guid VaultGuid => Guid.TryParse(VaultId, out var g) ? g : Guid.Empty;

        protected VaultDetailsResponse? vault;
        protected VaultUnlockResponse? unlockedVaultData;
        protected string masterPassword = "";
        protected bool isUnlocked = false;
        protected string errorMessage = "";
        
        // Zero-Knowledge : On stocke les données chiffrées, pas les données en clair
        protected List<VaultEntryViewModel> decryptedEntries = new();
        
        protected bool showShareModal = false;
        protected bool showHistoryModal = false; // Pour l'historique
        protected VaultEntryViewModel newEntry = new();
        
        private bool showCreateModal;
        private bool showDeleteModal;
        private Guid entryToDelete;
        private bool showDeleteVaultModal = false;
        private bool showEditVaultModal = false;

        
        private ModalCreateOrUpdateVaultEntry.VaultEntryModalMode modalMode;
        private VaultEntryViewModel? entryBeingEdited;
        
        private void AskDeleteEntry(Guid id)
        {
            entryToDelete = id;
            showDeleteModal = true;
        }
        
        private async void AskEditEntry(Guid id)
        {
            var entryDto = decryptedEntries.First(e => e.Identifier == id);

            // Copie pour éviter modification directe avant validation
            newEntry = new VaultEntryViewModel
            {
                Identifier = entryDto.Identifier,
                EncryptedData = entryDto.EncryptedData
            };
            
            entryBeingEdited = entryDto;
            modalMode = ModalCreateOrUpdateVaultEntry.VaultEntryModalMode.Edit;
            showCreateModal = true;
            
            // MASQUER le mot de passe affiché
            await JSRuntime.InvokeVoidAsync(
                "cryptoFunctions.hidePassword",
                id.ToString()
            );
        }

        
        private async Task ConfirmDeleteEntry()
        {
            // Récupération de l'email pour le log
            var userEmail = await GetCurrentUserEmail();
            
            // Génération du log chiffré
            var encryptedLog = await JSRuntime.InvokeAsync<string>(
                "cryptoFunctions.encryptDeleteLog",
                entryToDelete.ToString(),
                userEmail
            );
            
            await DeleteEntry(entryToDelete, encryptedLog);
            showDeleteModal = false;
        }
        
        private void CancelDelete()
        {
            showDeleteModal = false;
        }
        
        private void AskDeleteVault()
        {
            showDeleteVaultModal = true;
        }

        private void CancelDeleteVault()
        {
            showDeleteVaultModal = false;
        }

        private async Task ConfirmDeleteVault()
        {
            await VaultService.DeleteVaultAsync(VaultGuid);
            showDeleteVaultModal = false;

            // retour à la home après suppression
            Navigation.NavigateTo("/");
        }
        
        private void AskEditVault()
        {
            showEditVaultModal  = true;
        }

        private void CancelEditVault()
        {
            showEditVaultModal  = false;
        }

        private async Task ConfirmEditVault(UpdateVaultRequest request)
        {
            bool passwordChanged = !string.IsNullOrWhiteSpace(request.NewPassword);

            // Si changement de mot de passe → crypto JS
            if (passwordChanged)
            {
                // Utilisation de changeMasterPassword pour conserver la clé du coffre
                var crypto = await JSRuntime.InvokeAsync<CreateVault.CryptoResult>(
                    "cryptoFunctions.changeMasterPassword",
                    request.NewPassword);

                request.MasterSalt = crypto.MasterSalt;
                request.EncryptedKey = crypto.EncryptedKey;
            }

            await VaultService.UpdateVaultAsync(VaultGuid, request);

            showEditVaultModal = false;

            if (passwordChanged)
            {
                // SÉCURITÉ : On verrouille le coffre pour forcer l'utilisateur à se reconnecter
                // Cela garantit qu'il connait bien le nouveau mot de passe
                isUnlocked = false;
                decryptedEntries.Clear();
                unlockedVaultData = null;
                masterPassword = "";
                
                // On vide la clé en mémoire JS
                await JSRuntime.InvokeVoidAsync("cryptoFunctions.clearKey");
                
                errorMessage = "Mot de passe modifié. Veuillez déverrouiller le coffre à nouveau.";
            }

            // Recharge le vault (si pas de changement de mdp, on reste connecté, sinon on verra le formulaire de login)
            await LoadVault();
        }
        
        protected override async Task OnInitializedAsync()
        {
            await LoadVault();
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
                unlockedVaultData = await VaultService.UnlockVaultAsync(VaultGuid, masterPassword);

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
                    // Zero-Knowledge : On ne déchiffre plus ici !
                    // On passe directement les données chiffrées au ViewModel
                    // Le composant VaultEntry se chargera de l'affichage via JS
                    
                    var entryViewModel = new VaultEntryViewModel
                    {
                        Identifier = entryDto.Identifier,
                        EncryptedData = entryDto.EncryptedData
                    };
                    
                    decryptedEntries.Add(entryViewModel);
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
        
        private void OpenCreateModal()
        {
            newEntry = new();
            entryBeingEdited = null;
            modalMode = ModalCreateOrUpdateVaultEntry.VaultEntryModalMode.Create;
            showCreateModal = true;
        }

        private void CancelSaveEntry()
        {
            showCreateModal = false;
        }

        private async Task ConfirmSaveEntry()
        {
            await SaveEntry();
            showCreateModal = false;
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

        private async Task SaveEntry()
        {
            // Tout est récupéré et chiffré côté JS
            var encryptedData = await JSRuntime.InvokeAsync<string>(
                "cryptoFunctions.encryptEntryData",
                "vaultEntryTitleInput",
                "vaultEntryUsernameInput",
                "vaultEntryCommentInput",
                "vaultEntryUrlInput"
            );

            var encryptedPassword = await JSRuntime.InvokeAsync<string>(
                "cryptoFunctions.encryptInputValue",
                "vaultEntryPasswordInput"
            );
            
            // Récupération de l'email pour le log
            var userEmail = await GetCurrentUserEmail();

            if (newEntry.Identifier == Guid.Empty)
            {
                // ===== CREATE =====
                // Génération du log chiffré
                var encryptedLog = await JSRuntime.InvokeAsync<string>(
                    "cryptoFunctions.encryptCreateLog",
                    "vaultEntryTitleInput",
                    userEmail
                );

                var request = new CreateVaultEntryRequest
                {
                    VaultIdentifier = VaultGuid,
                    EncryptedData = encryptedData,
                    EncryptedPassword = encryptedPassword,
                    EncryptedLog = encryptedLog
                };

                var createdId = await VaultEntryService.CreateEntryAsync(request);

                decryptedEntries.Add(new VaultEntryViewModel
                {
                    Identifier = createdId,
                    EncryptedData = encryptedData
                });
            }
            else
            {
                // ===== UPDATE =====
                // Génération du log chiffré avec comparaison
                var encryptedLog = await JSRuntime.InvokeAsync<string>(
                    "cryptoFunctions.encryptUpdateLog",
                    entryBeingEdited.EncryptedData, // Ancienne donnée chiffrée
                    "vaultEntryTitleInput",
                    "vaultEntryUsernameInput",
                    "vaultEntryPasswordInput",
                    "vaultEntryCommentInput",
                    "vaultEntryUrlInput",
                    userEmail
                );

                var request = new UpdateVaultEntryRequest
                {
                    EntryIdentifier = newEntry.Identifier,
                    EncryptedData = encryptedData,
                    EncryptedPassword = string.IsNullOrWhiteSpace(encryptedPassword)
                        ? null
                        : encryptedPassword,
                    EncryptedLog = encryptedLog
                };

                await VaultEntryService.UpdateVaultEntryAsync(request);

                // Mise à jour locale (pas de reload serveur)
                var entry = decryptedEntries.First(e => e.Identifier == newEntry.Identifier);
                entry.EncryptedData = encryptedData;
            }

            newEntry = new();
        }

        // ShowPassword supprimé car géré par le composant VaultEntry

        private async Task DeleteEntry(Guid entryId, string encryptedLog)
        {
            await VaultEntryService.DeleteVaultEntryAsync(entryId, encryptedLog);

            decryptedEntries.RemoveAll(e => e.Identifier == entryId);

            StateHasChanged();
        }

        protected void ToggleShare()
        {
            showShareModal = !showShareModal;
        }
        
        protected void ToggleHistory()
        {
            showHistoryModal = !showHistoryModal;
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

        // Renommé pour refléter que ce n'est plus "Decrypted" mais un ViewModel d'affichage
        public class VaultEntryViewModel
        {
            public Guid Identifier { get; set; }
            public string EncryptedData { get; set; } = ""; // Contient Titre + Username chiffrés
            
            // Password n'est plus stocké ici par défaut, il est récupéré à la demande
            // On garde la propriété pour la compatibilité avec la méthode ShowPassword existante si besoin
            public string Password { get; set; } = ""; 
            
            // Ces propriétés ne sont plus utilisées pour le transport, mais peuvent servir de cache si on déchiffre
            public string Title { get; set; } = ""; 
            public string Username { get; set; } = "";
        }
        
        private void GoBack()
        {
            Navigation.NavigateTo("/");
        }
        
        private async Task LoadVault()
        {
            if (VaultGuid == Guid.Empty) 
            {
                errorMessage = "ID de coffre invalide.";
                return;
            }

            try
            {
                vault = await VaultService.GetVaultDetailsAsync(VaultGuid);
                // On ne recharge pas les entrées ici car elles sont chiffrées et nécessitent le mot de passe.
                // Si le coffre est déjà déverrouillé, les entrées sont déjà dans decryptedEntries.
            }
            catch (Exception ex)
            {
                errorMessage = "Impossible de charger les détails du coffre.";
                Logger.LogError(ex, "Error loading vault");
            }
        }

    }
}
