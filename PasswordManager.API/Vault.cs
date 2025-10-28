﻿namespace PasswordManager.API
{
    public class Vault
    {
        //clearData
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CreatorIdentifier { get; set; } = string.Empty; //Foreign Key for the Vault Creator
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool isShared { get; set; }

        public string MasterSalt { get; set; } //Salt used to created the Symetrique encryption key from the password
        public string Salt { get; set; } //Salt used to encrypt the vault entries
        public string encryptKey { get; set; }
        public string Password { get; set; }




        public HashSet<VaultEntry> Entries { get; set; } = [];
        public HashSet<AppUser> SharedUsers { get; set; } = [];
        public AppUser? Creator { get; set; }
       
    }
}
