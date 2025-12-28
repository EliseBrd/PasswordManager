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
                .Include(v => v.SharedUsers)
                .FirstOrDefaultAsync(v => v.Identifier == id.ToString());
        }

        public async Task<Vault?> GetByIdWithSharedUsersAsync(Guid id)
        {
            return await _context.Vaults
                .Include(v => v.SharedUsers)
                .FirstOrDefaultAsync(v => v.Identifier == id.ToString());
        }

        public async Task<IEnumerable<Vault>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Vaults
                .Where(v => v.SharedUsers.Any(u => u.Identifier == userId))
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
    }
}
