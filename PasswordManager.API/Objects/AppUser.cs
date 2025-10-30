using System.ComponentModel.DataAnnotations;

namespace PasswordManager.API.Objects
{
    public class AppUser
    {
        [Key]
        public Guid Identifier { get; set; } = Guid.NewGuid();

        [Required]
        public Guid entraId { get; set; } = Guid.Empty;

        // Relations
        public HashSet<Vault>? Vaults { get; set; }
        public HashSet<Vault>? SharedVaults { get; set; }
        public HashSet<VaultEntry>? Entries { get; set; }
    }
}
