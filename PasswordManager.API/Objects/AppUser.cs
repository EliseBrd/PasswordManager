using PasswordManager.API.Objects;

namespace PasswordManager.API
{
    public class AppUser
    {
        public Guid Identifier { get; set; } = default;
        public Guid entraId { get; set; } = default;
        public string Email { get; set; } = string.Empty;


        public HashSet<Vault> Vaults { get; set; } = [];
        public HashSet<VaultUserAccess> VaultAccesses { get; set; } = [];
        public HashSet<VaultEntry> Entries { get; set; } = [];
    }
}