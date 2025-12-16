using PasswordManager.API.Objects;

namespace PasswordManager.API
{
    public class AppUser
    {
        public Guid Identifier { get; set; } = default;
        public Guid entraId { get; set; } = default;


        public HashSet<Vault> Vaults { get; set; } = [];
        public HashSet<Vault> SharedVaults { get; set; } = [];
        public HashSet<VaultEntry> Entries { get; set; } = [];
    }
}