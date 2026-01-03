using System;

namespace PasswordManager.Dto.Vault.Requests
{
    public class CreateVaultEntryRequest
    {
        public Guid VaultIdentifier { get; set; }
        public string EncryptedData { get; set; } = string.Empty;     // Title, Username, etc.
        public string EncryptedPassword { get; set; } = string.Empty; // Password only
    }
}
