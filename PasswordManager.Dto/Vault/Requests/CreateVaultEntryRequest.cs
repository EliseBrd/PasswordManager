namespace PasswordManager.Dto.Vault.Requests
{
    public class CreateVaultEntryRequest
    {
        public string VaultIdentifier { get; set; } = string.Empty;
        public string EncryptedData { get; set; } = string.Empty;     // Title, Username, etc.
        public string EncryptedPassword { get; set; } = string.Empty; // Password only
    }
}
