using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using System.Text;
using Microsoft.JSInterop;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Web.Services;

namespace PasswordManager.Web.Components.Pages
{
    public partial class CreateVault : ComponentBase
    {
        [Inject] private IJSRuntime Js { get; set; } = default!;
        [Inject] private VaultService VaultService { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;

        // =========================
        // STATE
        // =========================
        private string _vaultName = "";
        private string _vaultPassword = "";

        // =========================
        // INPUT LIVE
        // =========================
        /*protected void OnPasswordInput(ChangeEventArgs e)
        {
            _vaultPassword = e.Value?.ToString() ?? "";
        }*/
        
        protected void OnPasswordInput(ChangeEventArgs e)
        {
            _vaultPassword = e.Value?.ToString() ?? "";
        }
        
        private void OnPasswordGenerated(string pwd)
        {
            _vaultPassword = pwd;
        }

        // =========================
        // CRÉATION DU COFFRE
        // =========================
        private async Task EventCreateVault()
        {
            if (!string.IsNullOrWhiteSpace(_vaultName) && !string.IsNullOrWhiteSpace(_vaultPassword))
            {
                // Generate crypto material client-side
                var cryptoResult = await Js.InvokeAsync<CryptoResult>("cryptoFunctions.createVaultCrypto", _vaultPassword);

                var request = new CreateVaultRequest
                {
                    Name = _vaultName,
                    Password = _vaultPassword, // Sent for authentication hashing only
                    MasterSalt = cryptoResult.MasterSalt,
                    EncryptedKey = cryptoResult.EncryptedKey
                };

                await VaultService.CreateVaultAsync(request);
                NavManager.NavigateTo("/");
            }
        }

        public class CryptoResult
        {
            public string MasterSalt { get; set; } = "";
            public string EncryptedKey { get; set; } = "";
        }
    }
}
