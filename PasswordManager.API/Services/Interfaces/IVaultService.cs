using PasswordManager.API.Objects;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;

namespace PasswordManager.API.Services.Interfaces
{
    public interface IVaultService
    {
        Task<IEnumerable<VaultSummaryResponse>> GetAccessibleVaultsAsync(Guid userId);
        Task<Vault?> GetVaultByIdAsync(Guid id);
        Task<Vault> CreateVaultAsync(string name, string password, Guid creatorId);
        Task<VaultEntry> CreateVaultEntryAsync(CreateVaultEntryRequest request, Guid creatorId);
        Task<string?> GetVaultEntryPasswordAsync(int entryId);
        Task<Vault?> AccessVaultAsync(Guid vaultId, string password);
        Task<bool> UpdateVaultAsync(Vault vault);
        Task<bool> DeleteAsync(Guid id);
    }
}
