using System;

namespace PasswordManager.API.Objects
{
    public class VaultLog
    {
        public Guid Identifier { get; set; } = Guid.NewGuid();
        
        public Guid VaultIdentifier { get; set; }
        public Vault Vault { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Données chiffrées (JSON : { Type, Text, UserEmail })
        public string EncryptedData { get; set; } = string.Empty;
        
        // Métadonnées de chiffrement (si nécessaire séparément, sinon inclus dans EncryptedData comme pour les entrées)
        // Dans votre projet actuel, EncryptedData semble contenir IV + Cypher + Tag concaténés.
        // On va garder la même logique pour rester cohérent.
    }
}
