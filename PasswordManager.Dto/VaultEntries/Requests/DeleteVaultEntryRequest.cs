using System;

namespace PasswordManager.Dto.VaultEntries.Requests
{
    public class DeleteVaultEntryRequest
    {
        public Guid EntryIdentifier { get; set; }
        public string EncryptedLog { get; set; } = string.Empty;
    }
}
