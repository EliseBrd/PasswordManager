/*
using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Repositories.Interfaces;

namespace PasswordManager.API.Repositories
{
    // Classe responsable des opérations directes sur la table VaultEntries.
    // Elle interagit avec la base de données via le DbContext
    public class VaultEntryRepository : IVaultEntryRepository
    {
        private readonly PasswordManagerDBContext _context;

        public VaultEntryRepository(PasswordManagerDBContext context)
        {
            _context = context;
        }

        // Retourne tous les entrés de coffres d'un coffre avec son identifiant
        public async Task<IEnumerable<VaultEntry>> GetEntriesByVaultIdAsync(Guid vaultId)
        {
            return await _context.VaultEntries
                .Where(e => e.VaultIdentifier == vaultId)
                .ToListAsync();
        }

        // Recherche une entrée de coffre par son identifiant GUID
        public async Task<VaultEntry?> GetByIdAsync(Guid id)
        {
            return await _context.VaultEntries.FindAsync(id);
        }

        // Ajoute une nouvelle entrée de coffre dans la base
        public async Task AddAsync(VaultEntry entry)
        {
            _context.VaultEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        // Modifie une entrée de coffre dans la base
        public async Task UpdateAsync(VaultEntry entry)
        {
            _context.VaultEntries.Update(entry);
            await _context.SaveChangesAsync();
        }

        // Supprime une entrée de coffre dans la base
        public async Task DeleteAsync(Guid id)
        {
            var existing = await _context.VaultEntries.FindAsync(id);
            if (existing != null)
            {
                _context.VaultEntries.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }

    }
}
*/
