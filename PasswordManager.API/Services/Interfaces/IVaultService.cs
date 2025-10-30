using PasswordManager.API.Objects;

namespace PasswordManager.API.Services.Interfaces
{
    // Contrat du service de gestion des Vaults
    // Le service contient la logique métier de l'application
    // Il utilise le repository pour accéder à la base de données
    public interface IVaultService
    {
        Task<IEnumerable<Vault>> GetAllVaultsAsync();
        Task<Vault?> GetVaultByIdAsync(Guid id);
        Task<Vault> CreateVaultAsync(string name, string masterSalt, string salt, Guid creatorId);
        Task<bool> UpdateVaultAsync(Vault vault);
        Task<bool> DeleteVaultAsync(Guid id);
    }
}
