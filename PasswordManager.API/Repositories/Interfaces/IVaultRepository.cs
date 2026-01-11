using PasswordManager.API.Objects;

namespace PasswordManager.API.Repositories.Interfaces
{
    public interface IVaultRepository
    {
        Task<IEnumerable<Vault>> GetAllAsync();
        Task<Vault?> GetByIdAsync(Guid id);
        Task<Vault?> GetByIdWithSharedUsersAsync(Guid id);
        Task<IEnumerable<Vault>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Vault vault);
        Task UpdateAsync(Vault vault);
        Task DeleteAsync(Guid id);
        Task AddUserAccessAsync(VaultUserAccess access);
        Task RemoveUserAccessAsync(Guid vaultId, Guid userId);
        Task UpdateUserAccessAsync(VaultUserAccess access);
    }
}
