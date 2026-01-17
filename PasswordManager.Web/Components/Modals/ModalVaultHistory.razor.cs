using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Web.Services;
using System.Text.Json;

namespace PasswordManager.Web.Components.Modals
{
    public partial class ModalVaultHistory : ComponentBase
    {
        [Inject] protected VaultService VaultService { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public Guid VaultId { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private bool isLoading = false;
        // Suppression de logsLoaded pour forcer le rechargement à chaque ouverture

        protected override async Task OnParametersSetAsync()
        {
            // On recharge à chaque fois que la modale s'ouvre
            if (IsOpen && VaultId != Guid.Empty)
            {
                // Petit délai pour laisser le temps au DOM (div logsContainer) d'être rendu si c'est la première ouverture
                // ou si Blazor vient de réafficher le composant.
                await Task.Delay(50); 
                await LoadLogs();
            }
        }

        private async Task LoadLogs()
        {
            isLoading = true;
            StateHasChanged();
            
            // On attend que le rendu se fasse pour afficher le spinner
            await Task.Delay(1);

            try
            {
                // 1. Récupérer les logs chiffrés depuis l'API
                var encryptedLogs = await VaultService.GetVaultLogsAsync(VaultId);

                // 2. Déchiffrer et afficher via JS (Zero-Knowledge)
                var logsForJs = encryptedLogs.Select(l => new 
                { 
                    identifier = l.Identifier, 
                    date = l.Date, 
                    encryptedData = l.EncryptedData 
                }).ToList();

                await JSRuntime.InvokeVoidAsync(
                    "cryptoFunctions.renderDecryptedLogs", 
                    logsForJs,
                    "logsContainer"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'historique : {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
    }
}
