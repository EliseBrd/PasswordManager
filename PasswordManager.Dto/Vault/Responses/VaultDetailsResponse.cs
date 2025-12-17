using System;
using System.Collections.Generic;

namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultDetailsResponse
    {
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid CreatorIdentifier { get; set; }
        public bool IsCreator { get; set; } // New property
        public bool IsShared { get; set; }
        public string MasterSalt { get; set; } = string.Empty;
        public string EncryptedKey { get; set; } = string.Empty;
        public List<VaultEntryDto> Entries { get; set; } = new();
    }

    public class VaultEntryDto
    {
        public string Identifier { get; set; } = string.Empty;
        public string EncryptedData { get; set; } = string.Empty; // Title, Username, etc.
    }
}
