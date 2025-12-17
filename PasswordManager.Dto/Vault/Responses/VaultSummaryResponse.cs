namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultSummaryResponse
    {
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsShared { get; set; }
    }
}
