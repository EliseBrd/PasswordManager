using PasswordManager.API.Objects;

namespace PasswordManager.API.Services.Interfaces
{
    // Contrat du service métier pour les VaultEntries
    // Il encapsule la logique et les règles d'application
    public interface IVaultEntryService
    {
        Task<IEnumerable<VaultEntry>> GetEntriesByVaultIdAsync(Guid vaultId);
        Task<VaultEntry?> GetByIdAsync(Guid id);
        Task<VaultEntry> CreateEntryAsync(VaultEntry entry);
        Task<bool> UpdateEntryAsync(VaultEntry entry);
        Task<bool> DeleteEntryAsync(Guid id);
    }
}
