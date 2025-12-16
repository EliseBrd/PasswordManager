using PasswordManager.API.Objects;

namespace PasswordManager.API.Services.Interfaces
{
    public interface IVaultService
    {
        Task<IEnumerable<Vault>> GetAllVaultsAsync();
        Task<Vault?> GetVaultByIdAsync(Guid id);
        Task<Vault> CreateVaultAsync(string name, string password, Guid creatorId);
        Task<Vault?> AccessVaultAsync(Guid vaultId, string password);
        Task<bool> UpdateVaultAsync(Vault vault);
        Task<bool> DeleteVaultAsync(Guid id);
    }
}
