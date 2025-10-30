using System.ComponentModel.DataAnnotations;

namespace PasswordManager.API.Objects
{
    public class VaultEntry
    {
        //Cler Data
        [Key]
        public Guid Identifier { get; set; } = Guid.NewGuid();
        public Guid VaultIdentifier { get; set; } = Guid.NewGuid(); //Foreign Key for the Vault 
        public Vault? Vault { get; set; }

        public Guid CreatorIdentifier { get; set; } = Guid.NewGuid(); //Foreign Key for the Vault Creator
        public AppUser? Creator { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Données chiffrées
        public string CypherPassword { get; set; } = string.Empty;
        public string CypherData { get; set; } = string.Empty;

        //Tag
        public string TagPasswords { get; set; } = string.Empty;
        public string TagData { get; set; } = string.Empty;

        //IV (vecteur d’initialisation)
        public string IVPassword { get; set; } = string.Empty;
        public string IVData { get; set; } = string.Empty;


       

        

       

        
    }
}
