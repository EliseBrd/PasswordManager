using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Repositories.Interfaces;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Responses;
using System.Security.Cryptography;
using BCrypt.Net;

namespace PasswordManager.API.Services
{
    public class VaultService : IVaultService
    {
        private readonly IVaultRepository _repository;
        private readonly PasswordManagerDBContext _context;
        private const int Pbkdf2Iterations = 100000;

        public VaultService(IVaultRepository repository, PasswordManagerDBContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<IEnumerable<VaultSummaryResponse>> GetAccessibleVaultsAsync(Guid userId)
        {
            var vaults = await _repository.GetByUserIdAsync(userId);
            return vaults.Select(v => new VaultSummaryResponse
            {
                Identifier = v.Identifier,
                Name = v.Name,
                Password = v.Password // Remember this is the HASH
            });
        }
        
        public async Task<Vault?> AccessVaultAsync(Guid vaultId, string password)
        {
            var vault = await _repository.GetByIdAsync(vaultId);
            if (vault == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, vault.Password)) return null;
            return vault;
        }

        public async Task<Vault> CreateVaultAsync(string name, string password, Guid creatorId)
        {
            var creator = await _context.Users.FindAsync(creatorId);
            if (creator == null)
                throw new InvalidOperationException("Creator user not found.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var masterSaltBytes = RandomNumberGenerator.GetBytes(64);
            var kek = DeriveKeyFromPassword(password, masterSaltBytes);
            var vaultKey = RandomNumberGenerator.GetBytes(32);
            var encryptedVaultKey = EncryptKeyAesGcm(vaultKey, kek);

            var vault = new Vault
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                MasterSalt = Convert.ToBase64String(masterSaltBytes),
                Password = hashedPassword,
                Salt = string.Empty,
                encryptKey = encryptedVaultKey,
                CreatorIdentifier = creatorId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                isShared = false,
                SharedUsers = new HashSet<AppUser> { creator } // Add creator to shared users
            };

            await _repository.AddAsync(vault);
            return vault;
        }

        public async Task<Vault?> GetVaultByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateVaultAsync(Vault vault)
        {
            var existing = await _repository.GetByIdAsync(new Guid(vault.Identifier));
            if (existing == null) return false;
            existing.Name = vault.Name;
            existing.LastUpdatedAt = DateTime.UtcNow;
            existing.isShared = vault.isShared;
            await _repository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }

        private byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        private string EncryptKeyAesGcm(byte[] dataToEncrypt, byte[] key)
        {
            using var aes = new AesGcm(key, 16);
            var nonce = RandomNumberGenerator.GetBytes(12);
            var tag = new byte[16];
            var ciphertext = new byte[dataToEncrypt.Length];
            aes.Encrypt(nonce, dataToEncrypt, ciphertext, tag);
            var result = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);
            return Convert.ToBase64String(result);
        }
    }
}
