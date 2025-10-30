using PasswordManager.API.Objects;

namespace PasswordManager.API.Repositories.Interfaces
{
    // Contrat du repository des Vaults.
    // Le repository gère UNIQUEMENT la communication avec la base de données.
    // Il ne contient PAS de logique métier, juste de la persistance (CRUD).

    public interface IVaultRepository
    {
        Task<IEnumerable<Vault>> GetAllAsync(); // doit pouvoir récupérer tous les vaults
        Task<Vault?> GetByIdAsync(Guid id); // doit pouvoir en récupérer un par ID
        Task AddAsync(Vault vault); // doit pouvoir en ajouter un
        Task UpdateAsync(Vault vault); // doit pouvoir en modifier un
        Task DeleteAsync(Guid id); // doit pouvoir en supprimer un
    }
}
