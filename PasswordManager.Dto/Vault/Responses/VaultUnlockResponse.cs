using System.Collections.Generic;

namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultUnlockResponse
    {
        public string MasterSalt { get; set; } = string.Empty;
        public string EncryptedKey { get; set; } = string.Empty;
        public List<VaultUnlockEntryDto> Entries { get; set; } = new();
    }

}
