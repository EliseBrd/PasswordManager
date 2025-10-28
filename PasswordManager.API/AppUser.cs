namespace PasswordManager.API
{
    public class AppUser
    {
        public string Identifier { get; set; } = string.Empty;
        public Guid entraId { get; set; } = default;


        public HashSet<Vault> Vaults { get; set; } = [];
        public HashSet<Vault> SharedVaults { get; set; } = [];
        public HashSet<VaultEntry> Entries { get; set; } = [];
    }
}
