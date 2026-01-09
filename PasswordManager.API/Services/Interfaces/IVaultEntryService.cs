using PasswordManager.API.Objects;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.VaultsEntries.Requests;

namespace PasswordManager.API.Services.Interfaces
{
    // Contrat du service métier pour les VaultEntries
    // Il encapsule la logique et les règles d'application
    public interface IVaultEntryService
    {
        Task<bool> UpdateEntryAsync(Guid entryId, string encryptedData, string encryptedPassword);
        Task<bool> DeleteEntryAsync(Guid id);
        Task<VaultEntry> CreateEntryAsync(CreateVaultEntryRequest request, Guid creatorId);
        Task<string?> GetEntryPasswordAsync(Guid entryId);
    }
}