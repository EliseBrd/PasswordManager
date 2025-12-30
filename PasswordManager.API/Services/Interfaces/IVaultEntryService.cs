using PasswordManager.API.Objects;
using PasswordManager.Dto.Vault.Requests;

namespace PasswordManager.API.Services.Interfaces
{
    // Contrat du service métier pour les VaultEntries
    // Il encapsule la logique et les règles d'application
    public interface IVaultEntryService
    {
        Task<bool> UpdateEntryAsync(VaultEntry entry);
        Task<bool> DeleteEntryAsync(int id);
        Task<VaultEntry> CreateEntryAsync(CreateVaultEntryRequest request, Guid creatorId);
        Task<string?> GetEntryPasswordAsync(int entryId);
    }
}
