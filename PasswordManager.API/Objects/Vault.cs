namespace PasswordManager.API.Objects
{
    public class Vault
    {
        //clearData
        public Guid Identifier { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public Guid CreatorIdentifier { get; set; } = default; //Foreign Key for the Vault Creator
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsShared { get; set; }

        public string MasterSalt { get; set; } //Salt used to created the Symetrique encryption key from the password
        public string Salt { get; set; } //Salt used to encrypt the vault entries
        public string EncryptKey { get; set; }
        public string Password { get; set; }




        public HashSet<VaultEntry> Entries { get; set; } = [];
        public HashSet<VaultUserAccess> UserAccesses { get; set; } = [];
        public AppUser? Creator { get; set; }
       
    }
}
