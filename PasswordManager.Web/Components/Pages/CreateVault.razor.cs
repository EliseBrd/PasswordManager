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
        private double _passwordEntropy = 0;
        private string _strengthLabel = "Faible";
        private bool _isGenerated = false;

        private double BarWidth
        {
            get
            {
                if (string.IsNullOrEmpty(_vaultPassword))
                    return 0; // rien afficher
                return _strengthLabel switch
                {
                    "Faible" => 25,
                    "Moyen" => 50,
                    "Fort" => 100,
                    _ => 0
                };
            }
        }

        private string StrengthColor
        {
            get
            {
                if (string.IsNullOrEmpty(_vaultPassword))
                    return "#d1d5db"; // gris clair pour “rien”
                return _strengthLabel switch
                {
                    "Faible" => "#e74c3c", // rouge
                    "Moyen" => "#f39c12", // orange
                    "Fort" => "#2ecc71", // vert
                    _ => "#d1d5db" // gris clair
                };
            }
        }

        // =========================
        // INPUT LIVE
        // =========================
        private void OnPasswordInput(ChangeEventArgs e)
        {
            _vaultPassword = e.Value?.ToString() ?? "";
            _isGenerated = false;
            UpdateEntropy();
        }

        // =========================
        // GÉNÉRATION
        // =========================
        private async Task GeneratePassword()
        {
            const string chars =
                "abcdefghijklmnopqrstuvwxyz" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "0123456789" +
                "!@#$%^&*()-_=+[]{}<>?";

            int length = 16;
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[bytes[i] % chars.Length]);
            }

            _vaultPassword = sb.ToString();
            UpdateEntropy();

            await Task.CompletedTask;
        }

        // =========================
        // ENTROPIE
        // =========================
        private void UpdateEntropy()
        {
            _passwordEntropy = CalculerEntropie(_vaultPassword);
            _strengthLabel = GetStrengthLabel(_passwordEntropy);
        }

        private double CalculerEntropie(string chaine)
        {
            if (string.IsNullOrEmpty(chaine))
                return 0;

            int longueur = chaine.Length;
            var frequences = new Dictionary<char, int>();

            foreach (char c in chaine)
            {
                frequences[c] = frequences.ContainsKey(c)
                    ? frequences[c] + 1
                    : 1;
            }

            double entropie = 0;

            foreach (var entry in frequences)
            {
                double p = (double)entry.Value / longueur;
                entropie += -p * Math.Log2(p);
            }

            return entropie * longueur; // bits totaux
        }

        private string GetStrengthLabel(double entropy)
        {
            if (entropy < 40) return "Faible";
            if (entropy < 60) return "Moyen";
            return "Fort";
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
