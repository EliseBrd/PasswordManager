using System;

namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultLogResponse
    {
        public Guid Identifier { get; set; }
        public DateTime Date { get; set; }
        public string EncryptedData { get; set; } = string.Empty;
    }
}
