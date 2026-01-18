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
using Microsoft.Extensions.Logging;

namespace PasswordManager.API.Services
{
    public class VaultService : IVaultService
    {
        private readonly IVaultRepository _repository;
        private readonly PasswordManagerDBContext _context;
        private readonly ILogger<VaultService> _logger;
        private const int Pbkdf2Iterations = 100000;

        public VaultService(IVaultRepository repository, PasswordManagerDBContext context, ILogger<VaultService> logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<VaultSummaryResponse>> GetAccessibleVaultsAsync(Guid userId)
        {
            _logger.LogInformation("User {UserId} requesting accessible vaults", userId);
            var vaults = await _repository.GetByUserIdAsync(userId);
            return vaults.Select(v => new VaultSummaryResponse
            {
                Identifier = v.Identifier,
                Name = v.Name,
                IsShared = v.IsShared
            });
        }
        
        public async Task<Vault?> AccessVaultAsync(Guid vaultId, string password)
        {
            var vault = await _repository.GetByIdAsync(vaultId);
            if (vault == null)
            {
                _logger.LogWarning("Access attempt failed: Vault {VaultId} not found", vaultId);
                return null;
            }
            
            if (!BCrypt.Net.BCrypt.Verify(password, vault.Password))
            {
                _logger.LogWarning("Access attempt failed: Invalid password for Vault {VaultId}", vaultId);
                return null;
            }

            _logger.LogInformation("Vault {VaultId} accessed successfully", vaultId);
            return vault;
        }
        
        public async Task<Vault> CreateVaultAsync(CreateVaultRequest request, Guid creatorId)
        {
            _logger.LogInformation("Creating new vault '{VaultName}' for user {UserId}", request.Name, creatorId);
            
            var creator = await _context.Users.FindAsync(creatorId);
            if (creator == null)
            {
                _logger.LogError("CreateVault failed: Creator user {UserId} not found", creatorId);
                throw new InvalidOperationException("Creator user not found.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var vault = new Vault
            {
                Identifier = Guid.NewGuid(),
                Name = request.Name,
                MasterSalt = request.MasterSalt,
                Password = hashedPassword,
                Salt = string.Empty,
                EncryptKey = request.EncryptedKey,
                CreatorIdentifier = creatorId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                IsShared = false
            };

            // Add creator access as Admin
            vault.UserAccesses.Add(new VaultUserAccess
            {
                VaultIdentifier = vault.Identifier,
                UserIdentifier = creatorId,
                IsAdmin = true
            });

            await _repository.AddAsync(vault);
            _logger.LogInformation("Vault {VaultId} created successfully", vault.Identifier);
            return vault;
        }

        public async Task<Vault?> GetVaultByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }
        
        public async Task<bool> UpdateVaultAsync(Guid vaultId, UpdateVaultRequest request, Guid userId)
        {
            var vault = await _repository.GetByIdAsync(vaultId);
            if (vault == null)
            {
                _logger.LogWarning("Update failed: Vault {VaultId} not found", vault.Identifier);
                return false;
            }

            if (vault.CreatorIdentifier != userId)
            {
                _logger.LogWarning("Update failed: User {UserId} not creator", userId);
                return false;
            }
            
            vault.Name = request.Name;
            vault.LastUpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(request.MasterSalt) ||
                    string.IsNullOrWhiteSpace(request.EncryptedKey))
                {
                    throw new InvalidOperationException("Crypto material missing");
                }

                vault.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                vault.MasterSalt = request.MasterSalt;
                vault.EncryptKey = request.EncryptedKey;
            }

            await _repository.UpdateAsync(vault);
            _logger.LogInformation("Vault {VaultId} updated", vault.Identifier);
            return true;
        }


        public async Task<bool> UpdateVaultSharingAsync(Guid vaultId, bool isShared, Guid requestingUserId)
        {
            var vault = await _repository.GetByIdAsync(vaultId);
            if (vault == null) return false;

            // Check if requester is admin
            var userAccess = vault.UserAccesses.FirstOrDefault(ua => ua.UserIdentifier == requestingUserId);
            if (userAccess == null || !userAccess.IsAdmin)
            {
                _logger.LogWarning("Unauthorized sharing update attempt on Vault {VaultId} by User {UserId}", vaultId, requestingUserId);
                return false;
            }

            vault.IsShared = isShared;
            await _repository.UpdateAsync(vault);
            _logger.LogInformation("Vault {VaultId} sharing status updated to {IsShared} by User {UserId}", vaultId, isShared, requestingUserId);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Delete failed: Vault {VaultId} not found", id);
                return false;
            }
            
            await _repository.DeleteAsync(id);
            _logger.LogInformation("Vault {VaultId} deleted", id);
            return true;
        }

        public async Task<bool> DeleteVaultAsync(Guid vaultId)
        {
            return await DeleteAsync(vaultId);
        }

        public async Task<bool> ShareVaultAsync(Guid vaultId, Guid userId)
        {
            return await UpdateVaultSharingAsync(vaultId, true, userId);
        }

        public async Task<bool> AddUserToVaultAsync(Guid vaultId, Guid userIdToAdd, bool isAdmin, Guid requestingUserId)
        {
            var vault = await _repository.GetByIdWithSharedUsersAsync(vaultId);
            if (vault == null || !vault.IsShared)
            {
                _logger.LogWarning("AddUserToVault failed: Vault {VaultId} not found or not shared", vaultId);
                return false;
            }

            // Check if requester is admin
            var requesterAccess = vault.UserAccesses.FirstOrDefault(ua => ua.UserIdentifier == requestingUserId);
            if (requesterAccess == null || !requesterAccess.IsAdmin)
            {
                _logger.LogWarning("AddUserToVault failed: User {UserId} is not admin of Vault {VaultId}", requestingUserId, vaultId);
                return false;
            }

            var userToAdd = await _context.Users.FindAsync(userIdToAdd);
            if (userToAdd == null)
            {
                _logger.LogWarning("AddUserToVault failed: User to add {UserIdToAdd} not found", userIdToAdd);
                return false;
            }

            // Check if user already has access
            if (vault.UserAccesses.Any(ua => ua.UserIdentifier == userIdToAdd))
            {
                _logger.LogWarning("AddUserToVault failed: User {UserIdToAdd} already has access to Vault {VaultId}", userIdToAdd, vaultId);
                return false;
            }

            var newAccess = new VaultUserAccess
            {
                VaultIdentifier = vaultId,
                UserIdentifier = userIdToAdd,
                IsAdmin = isAdmin
            };

            await _repository.AddUserAccessAsync(newAccess);
            
            _logger.LogInformation("User {UserIdToAdd} added to Vault {VaultId} (Admin: {IsAdmin})", userIdToAdd, vaultId, isAdmin);
            return true;
        }

        public async Task<bool> RemoveUserFromVaultAsync(Guid vaultId, Guid userIdToRemove, Guid requestingUserId)
        {
            var vault = await _repository.GetByIdWithSharedUsersAsync(vaultId);
            if (vault == null) return false;

            // Check if requester is admin
            var requesterAccess = vault.UserAccesses.FirstOrDefault(ua => ua.UserIdentifier == requestingUserId);
            if (requesterAccess == null || !requesterAccess.IsAdmin)
            {
                _logger.LogWarning("RemoveUserFromVault failed: User {UserId} is not admin", requestingUserId);
                return false;
            }

            var accessToRemove = vault.UserAccesses.FirstOrDefault(ua => ua.UserIdentifier == userIdToRemove);
            if (accessToRemove == null)
            {
                _logger.LogWarning("RemoveUserFromVault failed: User {UserIdToRemove} not found in vault", userIdToRemove);
                return false;
            }

            // Cannot remove the creator
            if (userIdToRemove == vault.CreatorIdentifier)
            {
                _logger.LogWarning("RemoveUserFromVault failed: Cannot remove creator from vault");
                return false;
            }

            await _repository.RemoveUserAccessAsync(vaultId, userIdToRemove);
            
            _logger.LogInformation("User {UserIdToRemove} removed from Vault {VaultId}", userIdToRemove, vaultId);
            return true;
        }

        public async Task<bool> UpdateUserAccessAsync(Guid vaultId, Guid userIdToUpdate, bool isAdmin, Guid requestingUserId)
        {
            var vault = await _repository.GetByIdWithSharedUsersAsync(vaultId);
            if (vault == null) return false;

            // Check if requester is admin
            var requesterAccess = vault.UserAccesses.FirstOrDefault(ua => ua.UserIdentifier == requestingUserId);
            if (requesterAccess == null || !requesterAccess.IsAdmin)
            {
                return false;
            }

            // Cannot change creator's admin status
            if (userIdToUpdate == vault.CreatorIdentifier)
            {
                return false;
            }

            var accessToUpdate = vault.UserAccesses.FirstOrDefault(ua => ua.UserIdentifier == userIdToUpdate);
            if (accessToUpdate == null)
            {
                return false;
            }

            accessToUpdate.IsAdmin = isAdmin;
            await _repository.UpdateUserAccessAsync(accessToUpdate);
            return true;
        }

        public async Task<IEnumerable<VaultLogResponse>> GetLogsAsync(Guid vaultId, Guid requestingUserId)
        {
            var vault = await _repository.GetByIdWithSharedUsersAsync(vaultId);
            if (vault == null)
            {
                return new List<VaultLogResponse>();
            }

            // Check if requester is admin
            var requesterAccess = vault.UserAccesses.FirstOrDefault(ua => ua.UserIdentifier == requestingUserId);
            if (requesterAccess == null || !requesterAccess.IsAdmin)
            {
                _logger.LogWarning("GetLogs failed: User {UserId} is not admin of Vault {VaultId}", requestingUserId, vaultId);
                return new List<VaultLogResponse>();
            }

            var logs = await _repository.GetLogsAsync(vaultId);
            return logs.Select(l => new VaultLogResponse
            {
                Identifier = l.Identifier,
                Date = l.Date,
                EncryptedData = l.EncryptedData
            });
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
        
        public static double CalculateEntropy(string pwd)
        {
            if (string.IsNullOrEmpty(pwd))
                return 0;

            int length = pwd.Length;
            var frequencies = new Dictionary<char, int>();

            foreach (char c in pwd)
            {
                frequencies[c] = frequencies.ContainsKey(c) ? frequencies[c] + 1 : 1;
            }

            double entropy = 0;
            foreach (var kvp in frequencies)
            {
                double p = (double)kvp.Value / length;
                entropy += -p * Math.Log2(p);
            }

            return entropy * length; // bits totaux
        }
    }
}
