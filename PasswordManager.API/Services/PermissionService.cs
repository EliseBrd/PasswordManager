using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Services.Interfaces;

namespace PasswordManager.API.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly PasswordManagerDBContext _context;

        public PermissionService(PasswordManagerDBContext context)
        {
            _context = context;
        }

        public async Task<bool> CanAccessVaultAsync(Guid userId, Guid vaultId)
        {
            return await _context.Vaults
                .AnyAsync(v => v.Identifier == vaultId && 
                               (v.CreatorIdentifier == userId || v.SharedUsers.Any(u => u.Identifier == userId)));
        }

        public async Task<bool> CanManageVaultAsync(Guid userId, Guid vaultId)
        {
            return await _context.Vaults
                .AnyAsync(v => v.Identifier == vaultId && v.CreatorIdentifier == userId);
        }

        public async Task<bool> CanAccessVaultEntryAsync(Guid userId, Guid entryId)
        {
            return await _context.VaultEntries
                .Include(e => e.Vault)
                .ThenInclude(v => v.SharedUsers)
                .Where(e => e.Identifier == entryId)
                .AnyAsync(e => e.CreatorIdentifier == userId || 
                               e.Vault.CreatorIdentifier == userId ||
                               e.Vault.SharedUsers.Any(u => u.Identifier == userId));
        }

        public async Task<bool> CanManageVaultEntryAsync(Guid userId, Guid entryId)
        {
            return await _context.VaultEntries
                .Include(e => e.Vault)
                .Where(e => e.Identifier == entryId)
                .AnyAsync(e => e.CreatorIdentifier == userId || e.Vault.CreatorIdentifier == userId);
        }
    }
}