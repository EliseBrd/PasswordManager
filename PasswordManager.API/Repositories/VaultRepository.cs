using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Repositories.Interfaces;

namespace PasswordManager.API.Repositories
{
    public class VaultRepository : IVaultRepository
    {
        private readonly PasswordManagerDBContext _context;

        public VaultRepository(PasswordManagerDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vault>> GetAllAsync()
        {
            return await _context.Vaults.ToListAsync();
        }

        public async Task<Vault?> GetByIdAsync(Guid id)
        {
            return await _context.Vaults
                .Include(v => v.Entries)
                .Include(v => v.UserAccesses)
                    .ThenInclude(ua => ua.User)
                .FirstOrDefaultAsync(v => v.Identifier == id);
        }

        public async Task<Vault?> GetByIdWithSharedUsersAsync(Guid id)
        {
            return await _context.Vaults
                .Include(v => v.UserAccesses)
                    .ThenInclude(ua => ua.User)
                .FirstOrDefaultAsync(v => v.Identifier == id);
        }

        public async Task<IEnumerable<Vault>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Vaults
                .Where(v => v.UserAccesses.Any(ua => ua.UserIdentifier == userId))
                .ToListAsync();
        }

        public async Task AddAsync(Vault vault)
        {
            _context.Vaults.Add(vault);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vault vault)
        {
            _context.Vaults.Update(vault);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var vault = await _context.Vaults.FindAsync(id);
            if (vault != null)
            {
                _context.Vaults.Remove(vault);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddUserAccessAsync(VaultUserAccess access)
        {
            _context.VaultUserAccesses.Add(access);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserAccessAsync(Guid vaultId, Guid userId)
        {
            var access = await _context.VaultUserAccesses
                .FirstOrDefaultAsync(ua => ua.VaultIdentifier == vaultId && ua.UserIdentifier == userId);
            
            if (access != null)
            {
                _context.VaultUserAccesses.Remove(access);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateUserAccessAsync(VaultUserAccess access)
        {
            _context.VaultUserAccesses.Update(access);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<VaultLog>> GetLogsAsync(Guid vaultId)
        {
            return await _context.VaultLogs
                .Where(l => l.VaultIdentifier == vaultId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();
        }
    }
}
