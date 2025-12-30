namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultUnlockEntryDto
    {
        public int Identifier { get; set; }
        public string EncryptedData { get; set; } = string.Empty;
    }
}