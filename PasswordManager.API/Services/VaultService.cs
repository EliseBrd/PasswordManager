using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Repositories.Interfaces;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;
using System.Security.Cryptography;
using BCrypt.Net;
using System;

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
                IsShared = v.isShared
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

        public async Task<VaultEntry> CreateVaultEntryAsync(CreateVaultEntryRequest request, Guid creatorId)
        {
            // Split EncryptedData
            var dataBytes = Convert.FromBase64String(request.EncryptedData);
            var dataIv = new byte[12];
            var dataTag = new byte[16];
            var dataCiphertext = new byte[dataBytes.Length - dataIv.Length - dataTag.Length];
            Buffer.BlockCopy(dataBytes, 0, dataIv, 0, dataIv.Length);
            Buffer.BlockCopy(dataBytes, dataIv.Length, dataCiphertext, 0, dataCiphertext.Length);
            Buffer.BlockCopy(dataBytes, dataIv.Length + dataCiphertext.Length, dataTag, 0, dataTag.Length);

            // Split EncryptedPassword
            var passwordBytes = Convert.FromBase64String(request.EncryptedPassword);
            var passwordIv = new byte[12];
            var passwordTag = new byte[16];
            var passwordCiphertext = new byte[passwordBytes.Length - passwordIv.Length - passwordTag.Length];
            Buffer.BlockCopy(passwordBytes, 0, passwordIv, 0, passwordIv.Length);
            Buffer.BlockCopy(passwordBytes, passwordIv.Length, passwordCiphertext, 0, passwordCiphertext.Length);
            Buffer.BlockCopy(passwordBytes, passwordIv.Length + passwordCiphertext.Length, passwordTag, 0, passwordTag.Length);

            var entry = new VaultEntry
            {
                VaultIdentifier = request.VaultIdentifier,
                CreatorIdentifier = creatorId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                IVData = Convert.ToBase64String(dataIv),
                CypherData = Convert.ToBase64String(dataCiphertext),
                TagData = Convert.ToBase64String(dataTag),
                IVPassword = Convert.ToBase64String(passwordIv),
                CypherPassword = Convert.ToBase64String(passwordCiphertext),
                TagPasswords = Convert.ToBase64String(passwordTag)
            };

            _context.VaultEntries.Add(entry);
            await _context.SaveChangesAsync();

            return entry;
        }

        public async Task<string?> GetVaultEntryPasswordAsync(int entryId)
        {
            var entry = await _context.VaultEntries.FindAsync(entryId);
            if (entry == null) return null;

            var ivBytes = Convert.FromBase64String(entry.IVPassword);
            var cypherBytes = Convert.FromBase64String(entry.CypherPassword);
            var tagBytes = Convert.FromBase64String(entry.TagPasswords);

            var combinedBytes = new byte[ivBytes.Length + cypherBytes.Length + tagBytes.Length];
            Buffer.BlockCopy(ivBytes, 0, combinedBytes, 0, ivBytes.Length);
            Buffer.BlockCopy(cypherBytes, 0, combinedBytes, ivBytes.Length, cypherBytes.Length);
            Buffer.BlockCopy(tagBytes, 0, combinedBytes, ivBytes.Length + cypherBytes.Length, tagBytes.Length);

            return Convert.ToBase64String(combinedBytes);
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
