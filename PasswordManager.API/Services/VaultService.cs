using PasswordManager.API.Objects;
using PasswordManager.API.Repositories.Interfaces;
using PasswordManager.API.Services.Interfaces;

namespace PasswordManager.API.Services
{
    // Implémentation du service de gestion des Vaults
    // Contient la logique métier de création et récupération de coffres
    public class VaultService : IVaultService
    {
        private readonly IVaultRepository _repository;

        public VaultService(IVaultRepository repository)
        {
            _repository = repository;
        }

        // Récupère tous les coffres existants
        public async Task<IEnumerable<Vault>> GetAllVaultsAsync()
        {
            return await _repository.GetAllAsync();
        }

        // Récupère un coffre spécifique par son identifiant
        public async Task<Vault?> GetVaultByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        // Crée un nouveau coffre avec les paramètres nécessaires
        public async Task<Vault> CreateVaultAsync(string name, string masterSalt, string salt, Guid creatorId)
        {
            var vault = new Vault
            {
                Identifier = Guid.NewGuid(),
                Name = name,
                MasterSalt = masterSalt,
                Salt = salt,
                CreatorIdentifier = creatorId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                isShared = false
            };

            await _repository.AddAsync(vault);
            return vault;
        }

        // Modifier un coffre avec les paramètres
        public async Task<bool> UpdateVaultAsync(Vault vault)
        {
            var existing = await _repository.GetByIdAsync(vault.Identifier);
            if (existing == null)
                return false;

            existing.Name = vault.Name;
            existing.LastUpdatedAt = DateTime.UtcNow;
            existing.isShared = vault.isShared;

            await _repository.UpdateAsync(existing);
            return true;
        }

        // Supprimer un coffre avec son identifiant
        public async Task<bool> DeleteVaultAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
