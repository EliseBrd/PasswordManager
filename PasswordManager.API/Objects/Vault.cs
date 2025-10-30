using System.ComponentModel.DataAnnotations;

namespace PasswordManager.API.Objects
{
    public class Vault
    {
        //clearData
        [Key]
        public Guid Identifier { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = string.Empty;

        public Guid CreatorIdentifier { get; set; } = Guid.NewGuid(); //Foreign Key for the Vault Creator
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool isShared { get; set; }


        public string MasterSalt { get; set; } = string.Empty; //Salt used to created the Symetrique encryption key from the password
        public string Salt { get; set; } = string.Empty; //Salt used to encrypt the vault entries
        public string encryptKey { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;


        public HashSet<VaultEntry>? Entries { get; set; }
        public HashSet<AppUser>? SharedUsers { get; set; }
        public AppUser? Creator { get; set; } // facultatif car on peut ne pas charger le créateur

    }
}
