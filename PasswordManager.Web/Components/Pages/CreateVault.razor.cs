using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using System.Text;
using Microsoft.JSInterop;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Web.Services;
using PasswordManager.Web.Components.Fragments;

namespace PasswordManager.Web.Components.Pages
{
    public partial class CreateVault : ComponentBase
    {
        [Inject] private IJSRuntime Js { get; set; } = default!;
        [Inject] private VaultService VaultService { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        
        private string _vaultName = "";
        private string _vaultPassword = "";
        private string _errorMessage = "";
        
        protected void OnPasswordInput(ChangeEventArgs e)
        {
            _vaultPassword = e.Value?.ToString() ?? "";
            _errorMessage = "";
        }
        
        private void OnPasswordGenerated(string pwd)
        {
            _vaultPassword = pwd;
            _errorMessage = "";
        }
        
        private async Task EventCreateVault()
        {
            _errorMessage = "";

            if (string.IsNullOrWhiteSpace(_vaultName))
            {
                _errorMessage = "Le nom du coffre est requis.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_vaultPassword))
            {
                _errorMessage = "Le mot de passe est requis.";
                return;
            }

            if (_vaultPassword.Length < 8)
            {
                _errorMessage = "Le mot de passe doit faire au moins 8 caractères.";
                return;
            }

            double entropy = PasswordTools.CalculateEntropy(_vaultPassword);
            if (entropy < 40)
            {
                _errorMessage = "Le mot de passe est trop faible (entropie insuffisante).";
                return;
            }

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

        public class CryptoResult
        {
            public string MasterSalt { get; set; } = "";
            public string EncryptedKey { get; set; } = "";
        }
    }
}