using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace PasswordManager.Web.Components.Fragments;

public partial class PasswordTools : ComponentBase
{
    // =========================
    // PARAMÈTRES
    // =========================
    [Parameter] public string Password { get; set; } = "";
    
    [Parameter] public EventCallback<string> OnGenerated { get; set; }

    // =========================
    // ÉTAT INTERNE
    // =========================
    private double _passwordEntropy = 0;
    private string _strengthLabel = "Faible";

    // =========================
    // BARRE & COULEUR
    // =========================
    private double BarWidth
    {
        get
        {
            if (string.IsNullOrEmpty(Password))
                return 0;
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
            if (string.IsNullOrEmpty(Password))
                return "#d1d5db"; // gris clair
            return _strengthLabel switch
            {
                "Faible" => "#e74c3c", // rouge
                "Moyen" => "#f39c12", // orange
                "Fort" => "#2ecc71", // vert
                _ => "#d1d5db"
            };
        }
    }

    // =========================
    // MÉTHODES
    // =========================
    protected override void OnParametersSet()
    {
        UpdateEntropy();
    }

    private void UpdateEntropy()
    {
        _passwordEntropy = CalculateEntropy(Password);
        _strengthLabel = GetStrengthLabel(_passwordEntropy);
    }

    private double CalculateEntropy(string pwd)
    {
        if (string.IsNullOrEmpty(pwd))
            return 0;

        int length = pwd.Length;
        var frequencies = new Dictionary<char, int>();

        foreach (char c in pwd)
        {
            frequencies[c] = frequencies.ContainsKey(c) ? frequencies[c] + 1 : 1;
        }

        double entropy = 0;
        foreach (var kvp in frequencies)
        {
            double p = (double)kvp.Value / length;
            entropy += -p * Math.Log2(p);
        }

        return entropy * length; // bits totaux
    }

    private string GetStrengthLabel(double entropy)
    {
        if (entropy < 40) return "Faible";
        if (entropy < 60) return "Moyen";
        return "Fort";
    }

    // =========================
    // GÉNÉRATION DE MOT DE PASSE
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

        Password = sb.ToString();
        UpdateEntropy();

        // NOTIFICATION UNIQUEMENT À LA GÉNÉRATION
        await OnGenerated.InvokeAsync(Password);
    }
    
}