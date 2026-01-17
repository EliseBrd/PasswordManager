using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Dto.Vault.Requests;

public class UpdateVaultRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    // null = pas de changement de mot de passe
    [StringLength(100, MinimumLength = 8)]
    public string? NewPassword { get; set; }

    public string? MasterSalt { get; set; }
    public string? EncryptedKey { get; set; }
}