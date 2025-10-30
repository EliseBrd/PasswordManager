namespace PasswordManager.Dto.Vault
{
    public class VaultEntryCreateDto
    {
        public string VaultIdentifier { get; set; } = string.Empty;

        // Données chiffrées
        public string CypherPassword { get; set; } = string.Empty;
        public string CypherData { get; set; } = string.Empty;

        // Tags d’intégrité
        public string TagPasswords { get; set; } = string.Empty;
        public string TagData { get; set; } = string.Empty;

        // IV (vecteurs d’initialisation)
        public string IVPassword { get; set; } = string.Empty;
        public string IVData { get; set; } = string.Empty;
    }
}
