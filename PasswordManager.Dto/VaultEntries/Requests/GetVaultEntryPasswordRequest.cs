using System;

namespace PasswordManager.Dto.VaultsEntries.Requests
{
    public class GetVaultEntryPasswordRequest
    {
        public Guid EntryIdentifier { get; set; }
        public string EncryptedLog { get; set; } = string.Empty;
    }
}
