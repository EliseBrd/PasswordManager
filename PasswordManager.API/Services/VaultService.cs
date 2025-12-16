using PasswordManager.API.Objects;
using PasswordManager.API.Repositories.Interfaces;
using PasswordManager.API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace PasswordManager.API.Services
{
    public class VaultService : IVaultService
    {
        private readonly IVaultRepository _repository;
        private const int Pbkdf2Iterations = 100000; // Stronger iteration count for key derivation

        public VaultService(IVaultRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<Vault?> AccessVaultAsync(Guid vaultId, string password)
        {
            var vault = await _repository.GetByIdAsync(vaultId);
            if (vault == null)
            {
                return null; // Vault not found
            }

            // Verify the provided password against the stored hash
            if (!BCrypt.Net.BCrypt.Verify(password, vault.Password))
            {
                return null; // Invalid password
            }

            return vault;
        }

        public async Task<Vault> CreateVaultAsync(string name, string password, Guid creatorId)
        {
            // 1. Hash the password for authentication (using BCrypt)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // 2. Generate a Master Salt
            var masterSaltBytes = RandomNumberGenerator.GetBytes(64);
            var masterSalt = Convert.ToBase64String(masterSaltBytes);

            // 3. Generate the Vault's actual encryption key (random 32 bytes)
            // This key will be used to encrypt the vault's entries.
            var vaultKey = RandomNumberGenerator.GetBytes(32);

            // 4. Derive a Key Encryption Key (KEK) from the user's password and MasterSalt
            // We use this KEK to encrypt the random vaultKey.
            var kek = DeriveKeyFromPassword(password, masterSaltBytes);

            // 5. Encrypt the vaultKey using AES-GCM with the KEK
            var encryptedVaultKey = EncryptKeyAesGcm(vaultKey, kek);

            var vault = new Vault
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                MasterSalt = masterSalt,
                Password = hashedPassword, 
                Salt = string.Empty, 
                encryptKey = encryptedVaultKey, // Stores the encrypted random vault key
                CreatorIdentifier = creatorId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                isShared = false
            };

            await _repository.AddAsync(vault);
            return vault;
        }

        public async Task<IEnumerable<Vault>> GetAllVaultsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Vault?> GetVaultByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateVaultAsync(Vault vault)
        {
            var existing = await _repository.GetByIdAsync(new Guid(vault.Identifier));
            if (existing == null)
                return false;

            existing.Name = vault.Name;
            existing.LastUpdatedAt = DateTime.UtcNow;
            existing.isShared = vault.isShared;

            await _repository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteVaultAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        // Helper to derive a 32-byte key from password and salt
        private byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32); // 32 bytes for AES-256
        }

        // Helper to encrypt data using AES-GCM
        private string EncryptKeyAesGcm(byte[] dataToEncrypt, byte[] key)
        {
            using var aes = new AesGcm(key, 16); // 16 bytes tag size is standard

            var nonce = RandomNumberGenerator.GetBytes(12); // 12 bytes nonce is standard for GCM
            var tag = new byte[16];
            var ciphertext = new byte[dataToEncrypt.Length];

            aes.Encrypt(nonce, dataToEncrypt, ciphertext, tag);

            // Combine Nonce + Ciphertext + Tag
            var result = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);

            return Convert.ToBase64String(result);
        }
    }
}
