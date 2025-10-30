using PasswordManager.API.Objects;

namespace PasswordManager.API
{
    public class VaultEntry
    {
        //Cler Data
        public int Identifier { get; set; }
        public string VaultIdentifier { get; set; } = string.Empty; //Foreign Key for the Vault 

        public Guid CreatorIdentifier { get; set; } = default; //Foreign Key for the Vault Creator
        public AppUser? Creator { get; set; }
        public Vault? Vault { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        //Cyphers
        public string CypherPassword { get; set; }
        public string CypherData { get; set; }

        //Tag
        public string TagPasswords { get; set; }
        public string TagData { get; set; }

        //IV
        public string IVPassword { get; set; }
        public string IVData { get; set; }


       

        

       

        
    }
}
