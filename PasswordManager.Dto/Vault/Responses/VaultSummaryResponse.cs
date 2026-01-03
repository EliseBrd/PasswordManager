using System;

namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultSummaryResponse
    {
        public Guid Identifier { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsShared { get; set; }
    }
}
