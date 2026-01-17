using System;

namespace PasswordManager.Dto.VaultsEntries.Requests
{
    public class CreateVaultEntryRequest
    {
        public Guid VaultIdentifier { get; set; }
        public string EncryptedData { get; set; } = string.Empty;     // Title, Username, etc.
        public string EncryptedPassword { get; set; } = string.Empty; // Password only
        
        // Nouveau champ pour le log d'audit chiffré
        public string EncryptedLog { get; set; } = string.Empty;
    }
}
