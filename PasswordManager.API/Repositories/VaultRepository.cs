using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Repositories.Interfaces;

namespace PasswordManager.API.Repositories
{
    // Implémentation concrète du repository des Vaults.
    // Ici, on utilise Entity Framework Core pour accéder à SQLite
    public class VaultRepository : IVaultRepository
    {
        private readonly PasswordManagerDBContext _context;

        public VaultRepository(PasswordManagerDBContext context)
        {
            _context = context;
        }

        // Retourne tous les coffres existants
        public async Task<IEnumerable<Vault>> GetAllAsync()
        {
            return await _context.Vaults.ToListAsync();
        }

        // Recherche un coffre par son identifiant GUID
        public async Task<Vault?> GetByIdAsync(Guid id)
        {
            return await _context.Vaults.FirstOrDefaultAsync(v => v.Identifier == id);
        }

        // Ajoute un nouveau coffre dans la base
        public async Task AddAsync(Vault vault)
        {
            _context.Vaults.Add(vault);
            await _context.SaveChangesAsync();
        }

        // Met à jour un coffre existant
        public async Task UpdateAsync(Vault vault)
        {
            _context.Vaults.Update(vault);
            await _context.SaveChangesAsync();
        }


        // Supprime un coffre à partir de son identifiant
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
