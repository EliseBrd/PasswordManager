namespace PasswordManager.API.Objects
{
    public class VaultUserAccess
    {
        public Guid VaultIdentifier { get; set; }
        public Vault Vault { get; set; }

        public Guid UserIdentifier { get; set; }
        public AppUser User { get; set; }

        public bool IsAdmin { get; set; }
    }
}
