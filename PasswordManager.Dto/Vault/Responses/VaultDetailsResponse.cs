using System;
using System.Collections.Generic;
using PasswordManager.Dto.User;

namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultDetailsResponse
    {
        public Guid Identifier { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CreatorIdentifier { get; set; }
        public bool IsCreator { get; set; }
        public bool IsShared { get; set; }
        public List<UserSummaryResponse> SharedWith { get; set; } = new();
    }
}
