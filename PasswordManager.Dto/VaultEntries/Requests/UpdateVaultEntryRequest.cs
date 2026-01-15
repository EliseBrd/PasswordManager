namespace PasswordManager.Dto.VaultsEntries.Requests;

public class UpdateVaultEntryRequest
{
    public Guid EntryIdentifier { get; set; }

    public string EncryptedData { get; set; } = string.Empty; // Title, Username, etc.

    public string EncryptedPassword { get; set; } = string.Empty; // Password only
}