using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using System.Text;
using Microsoft.JSInterop;

namespace PasswordManager.Web.Components.Pages
{
    public partial class CreateVault : ComponentBase
    {
        [Inject]
        protected IJSRuntime JS { get; set; } = default!;
        // =========================
        // STATE
        // =========================
        string vaultPassword = "";
        double passwordEntropy = 0;
        string strengthLabel = "Faible";
        bool isGenerated = false;
        double BarWidth
        {
            get
            {
                if (string.IsNullOrEmpty(vaultPassword))
                    return 0; // rien afficher
                return strengthLabel switch
                {
                    "Faible" => 25,
                    "Moyen" => 50,
                    "Fort" => 100,
                    _ => 0
                };
            }
        }

        string StrengthColor
        {
            get
            {
                if (string.IsNullOrEmpty(vaultPassword))
                    return "#d1d5db"; // gris clair pour “rien”
                return strengthLabel switch
                {
                    "Faible" => "#e74c3c", // rouge
                    "Moyen" => "#f39c12", // orange
                    "Fort" => "#2ecc71", // vert
                    _ => "#d1d5db" // gris clair
                };
            }
        }

        async Task Log(string message)
        {
            await JS.InvokeVoidAsync("console.log", message);
        }

        // =========================
        // INPUT LIVE
        // =========================
        void OnPasswordInput(ChangeEventArgs e)
        {
            vaultPassword = e.Value?.ToString() ?? "";
            isGenerated = false;
            UpdateEntropy();
        }

        // =========================
        // GÉNÉRATION
        // =========================
        private async Task GeneratePassword()
        {
            await Log("➡ GeneratePassword() appelé");

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

            vaultPassword = sb.ToString();
            UpdateEntropy();

            await Task.CompletedTask;
        }

        // =========================
        // ENTROPIE
        // =========================
        void UpdateEntropy()
        {
            passwordEntropy = CalculerEntropie(vaultPassword);
            strengthLabel = GetStrengthLabel(passwordEntropy);
        }

        double CalculerEntropie(string chaine)
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

        string GetStrengthLabel(double entropy)
        {
            if (entropy < 40) return "Faible";
            if (entropy < 60) return "Moyen";
            return "Fort";
        }

        // =========================
        // CRÉATION DU COFFRE
        // =========================
        async Task eventCreateVault()
        {
            // TODO : appel API avec vaultPassword
            NavigationManager.NavigateTo("/");
            await Task.CompletedTask;
        }
    }
}
