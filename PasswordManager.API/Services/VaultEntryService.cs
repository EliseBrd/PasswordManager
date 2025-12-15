//using PasswordManager.API.Objects;
//using PasswordManager.API.Repositories.Interfaces;
//using PasswordManager.API.Services.Interfaces;
//
//namespace PasswordManager.API.Services
//{
//    // Service qui contient la logique métier pour les entrées de coffre
//    // Il utilise le repository pour accéder à la base
//    public class VaultEntryService : IVaultEntryService
//    {
//        private readonly IVaultEntryRepository _repository;
//
//        public VaultEntryService(IVaultEntryRepository repository)
//        {
//            _repository = repository;
//        }
//
//        // Récupère tous les entrées de coffres d'un coffre avec son id
//        public async Task<IEnumerable<VaultEntry>> GetEntriesByVaultIdAsync(Guid vaultId)
//            => await _repository.GetEntriesByVaultIdAsync(vaultId);
//
//        // Recherche une entrée de coffre par son identifiant GUID
//        public async Task<VaultEntry?> GetByIdAsync(Guid id)
//            => await _repository.GetByIdAsync(id);
//
//        // Ajoute une nouvelle entrée de coffre dans la base
//        public async Task<VaultEntry> CreateEntryAsync(VaultEntry entry)
//        {
//            //entry.Identifier = Guid.NewGuid();
//            entry.CreatedAt = DateTime.UtcNow;
//            entry.LastUpdatedAt = DateTime.UtcNow;
//
//            await _repository.AddAsync(entry);
//            return entry;
//        }
//
//        // Modifie une entrée de coffre dans la base
//        //public async Task<bool> UpdateEntryAsync(VaultEntry entry)
//        //{
//        //    var existing = await _repository.GetByIdAsync(entry.Identifier);
//        //    if (existing == null)
//        //        return false;
////
//        //    existing.CypherPassword = entry.CypherPassword;
//        //    existing.CypherData = entry.CypherData;
//        //    existing.TagData = entry.TagData;
//        //    existing.TagPasswords = entry.TagPasswords;
//        //    existing.IVData = entry.IVData;
//        //    existing.IVPassword = entry.IVPassword;
//        //    existing.LastUpdatedAt = DateTime.UtcNow;
////
//        //    await _repository.UpdateAsync(existing);
//        //    return true;
//        //}
//
//        // Supprimer une entrée de coffre dans la base
//        public async Task<bool> DeleteEntryAsync(Guid id)
//        {
//            var existing = await _repository.GetByIdAsync(id);
//            if (existing == null)
//                return false;
//
//            await _repository.DeleteAsync(id);
//            return true;
//        }
//    }
//}
