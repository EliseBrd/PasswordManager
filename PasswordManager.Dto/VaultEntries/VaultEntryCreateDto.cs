namespace PasswordManager.Dto.VaultEntries
{
    public class VaultEntryCreateDto
    {
        public string Name { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Notes { get; set; } = "";
        public Guid VaultId { get; set; }
    }
}
