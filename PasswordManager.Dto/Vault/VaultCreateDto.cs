namespace PasswordManager.Dto.Vault
{
    public class VaultCreateDto
    {
        public string Name { get; set; } = string.Empty;

        // Ces valeurs sont générées côté client (Blazor)
        public string MasterSalt { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;

        public Guid CreatorId { get; set; } // test
    }
}
