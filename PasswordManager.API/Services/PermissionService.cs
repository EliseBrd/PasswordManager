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
            return await _context.VaultUserAccesses
                .AnyAsync(ua => ua.VaultIdentifier == vaultId && ua.UserIdentifier == userId);
        }

        public async Task<bool> CanManageVaultAsync(Guid userId, Guid vaultId)
        {
            return await _context.VaultUserAccesses
                .AnyAsync(ua => ua.VaultIdentifier == vaultId && ua.UserIdentifier == userId && ua.IsAdmin);
        }

        public async Task<bool> CanAccessVaultEntryAsync(Guid userId, Guid entryId)
        {
            // Correction : On passe par VaultUserAccesses directement pour éviter l'erreur sur SharedUsers
            // On vérifie s'il existe un accès pour cet utilisateur sur le coffre qui contient l'entrée
            return await _context.VaultEntries
                .Where(e => e.Identifier == entryId)
                .Join(_context.VaultUserAccesses,
                    entry => entry.VaultIdentifier,
                    access => access.VaultIdentifier,
                    (entry, access) => access)
                .AnyAsync(access => access.UserIdentifier == userId);
        }

        public async Task<bool> CanManageVaultEntryAsync(Guid userId, Guid entryId)
        {
            return await CanAccessVaultEntryAsync(userId, entryId);
        }
    }
}