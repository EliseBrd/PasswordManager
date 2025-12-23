using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Dto.Vault.Requests
{
    public class CreateVaultRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        public string MasterSalt { get; set; } = string.Empty;
        public string EncryptedKey { get; set; } = string.Empty;
    }
}
