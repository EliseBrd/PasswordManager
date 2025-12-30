using PasswordManager.API.Objects;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;

namespace PasswordManager.API.Services.Interfaces
{
    public interface IVaultService
    {
        Task<IEnumerable<VaultSummaryResponse>> GetAccessibleVaultsAsync(Guid userId);
        Task<Vault?> GetVaultByIdAsync(Guid id);
        Task<Vault> CreateVaultAsync(CreateVaultRequest request, Guid creatorId);
        Task<Vault?> AccessVaultAsync(Guid vaultId, string password);
        Task<bool> UpdateVaultAsync(Vault vault);
        Task<bool> UpdateVaultSharingAsync(Guid vaultId, bool isShared, Guid requestingUserId);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ShareVaultAsync(Guid vaultId, Guid userId);
        Task<bool> AddUserToVaultAsync(Guid vaultId, Guid userIdToAdd, Guid requestingUserId);
        Task<bool> RemoveUserFromVaultAsync(Guid vaultId, Guid userIdToRemove, Guid requestingUserId);
    }
}
