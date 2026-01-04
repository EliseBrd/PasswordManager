using System;

namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultUnlockEntryDto
    {
        public Guid Identifier { get; set; }
        public string EncryptedData { get; set; } = string.Empty;
    }
}
