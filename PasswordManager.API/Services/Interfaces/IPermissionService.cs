
namespace PasswordManager.API.Services.Interfaces;

    public interface IPermissionService
{
    public Task<bool> CanAccessVaultAsync(Guid userId, Guid vaultId);
    public Task<bool> CanManageVaultAsync(Guid userId, Guid vaultId);

    public Task<bool> CanAccessVaultEntryAsync(Guid userId, Guid entryId);
    public Task<bool> CanManageVaultEntryAsync(Guid userId, Guid entryId);
}
