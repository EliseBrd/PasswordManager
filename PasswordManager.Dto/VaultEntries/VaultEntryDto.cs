namespace PasswordManager.Dto.Vault
{
    public class VaultEntryDto
    {
        public int Identifier { get; set; }
        public string VaultIdentifier { get; set; } = string.Empty;

        public string CypherPassword { get; set; } = string.Empty;
        public string CypherData { get; set; } = string.Empty;
        public string TagPasswords { get; set; } = string.Empty;
        public string TagData { get; set; } = string.Empty;
        public string IVPassword { get; set; } = string.Empty;
        public string IVData { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
