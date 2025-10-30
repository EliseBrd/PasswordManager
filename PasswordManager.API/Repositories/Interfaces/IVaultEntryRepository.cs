using PasswordManager.API.Objects;

namespace PasswordManager.API.Repositories.Interfaces
{
    // Contrat pour les opérations CRUD sur les entrées de coffre (VaultEntry)
    // Gère uniquement l'accès aux données (base SQLite via EF Core)
    public interface IVaultEntryRepository
    {
        Task<IEnumerable<VaultEntry>> GetEntriesByVaultIdAsync(Guid vaultId); // doit pouvoir récupérer tous les vaultsEntry d'un Vault avec son identifiant
        Task<VaultEntry?> GetByIdAsync(Guid id);  // doit pouvoir en récupérer un vaultEntry par ID
        Task AddAsync(VaultEntry entry); // doit pouvoir en ajouter un
        Task UpdateAsync(VaultEntry entry); // doit pouvoir en modifier un
        Task DeleteAsync(Guid id); // doit pouvoir en supprimer un
    }
}
