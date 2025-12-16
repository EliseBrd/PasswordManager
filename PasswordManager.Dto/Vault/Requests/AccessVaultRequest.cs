using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Dto.Vault.Requests
{
    public class AccessVaultRequest
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
