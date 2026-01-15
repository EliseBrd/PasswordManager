using PasswordManager.Dto.User;

namespace PasswordManager.Dto.Vault.Responses
{
    public class VaultUserResponse : UserSummaryResponse
    {
        public bool IsAdmin { get; set; }
    }
}
