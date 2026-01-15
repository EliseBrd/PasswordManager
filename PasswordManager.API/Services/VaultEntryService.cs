using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PasswordManager.API;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.VaultsEntries.Requests;

namespace PasswordManager.API.Services;
public class VaultEntryService : IVaultEntryService
{
    private readonly PasswordManagerDBContext _context;
    private readonly ILogger<VaultEntryService> _logger;

    public VaultEntryService(PasswordManagerDBContext context, ILogger<VaultEntryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VaultEntry> CreateEntryAsync(CreateVaultEntryRequest request, Guid creatorId)
    {
        _logger.LogInformation("Creating new vault entry for Vault {VaultId} by User {UserId}", request.VaultIdentifier, creatorId);

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
            Identifier = Guid.NewGuid(),
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

        _logger.LogInformation("Vault entry {EntryId} created successfully in Vault {VaultId}", entry.Identifier, request.VaultIdentifier);
        return entry;
    }
    
    public async Task<string?> GetEntryPasswordAsync(Guid entryId)
    {
        var entry = await _context.VaultEntries.FindAsync(entryId);
        if (entry == null)
        {
            _logger.LogWarning("GetEntryPassword failed: Entry {EntryId} not found", entryId);
            return null;
        }

        var ivBytes = Convert.FromBase64String(entry.IVPassword);
        var cypherBytes = Convert.FromBase64String(entry.CypherPassword);
        var tagBytes = Convert.FromBase64String(entry.TagPasswords);

        var combinedBytes = new byte[ivBytes.Length + cypherBytes.Length + tagBytes.Length];
        Buffer.BlockCopy(ivBytes, 0, combinedBytes, 0, ivBytes.Length);
        Buffer.BlockCopy(cypherBytes, 0, combinedBytes, ivBytes.Length, cypherBytes.Length);
        Buffer.BlockCopy(tagBytes, 0, combinedBytes, ivBytes.Length + cypherBytes.Length, tagBytes.Length);

        _logger.LogInformation("Password retrieved for entry {EntryId}", entryId);
        return Convert.ToBase64String(combinedBytes);
    }

    public async Task<bool> DeleteEntryAsync(Guid id)
    {
        var entry = await _context.VaultEntries.FindAsync(id);
        if (entry == null)
        {
            _logger.LogWarning("DeleteEntry failed: Entry {EntryId} not found", id);
            return false;
        }
        _context.VaultEntries.Remove(entry);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Vault entry {EntryId} deleted", id);
        return true;
    }
    
    public async Task<bool> UpdateEntryAsync(
        Guid entryId,
        string encryptedData,
        string? encryptedPassword)
    {
        var entry = await _context.VaultEntries
            .FirstOrDefaultAsync(e => e.Identifier == entryId);

        if (entry == null)
            return false;

        // ===== SPLIT EncryptedData =====
        var dataBytes = Convert.FromBase64String(encryptedData);
        var dataIv = new byte[12];
        var dataTag = new byte[16];
        var dataCiphertext = new byte[dataBytes.Length - dataIv.Length - dataTag.Length];

        Buffer.BlockCopy(dataBytes, 0, dataIv, 0, dataIv.Length);
        Buffer.BlockCopy(dataBytes, dataIv.Length, dataCiphertext, 0, dataCiphertext.Length);
        Buffer.BlockCopy(dataBytes, dataIv.Length + dataCiphertext.Length, dataTag, 0, dataTag.Length);

        // ===== UPDATE ENTITY =====
        // ===== data (toujours) =====
        entry.IVData = Convert.ToBase64String(dataIv);
        entry.CypherData = Convert.ToBase64String(dataCiphertext);
        entry.TagData = Convert.ToBase64String(dataTag);

        // ===== password (uniquement si fourni) =====
        if (!string.IsNullOrEmpty(encryptedPassword))
        {
            // ===== SPLIT EncryptedPassword =====
            var passwordBytes = Convert.FromBase64String(encryptedPassword);
            var passwordIv = new byte[12];
            var passwordTag = new byte[16];
            var passwordCiphertext = new byte[passwordBytes.Length - passwordIv.Length - passwordTag.Length];

            Buffer.BlockCopy(passwordBytes, 0, passwordIv, 0, passwordIv.Length);
            Buffer.BlockCopy(passwordBytes, passwordIv.Length, passwordCiphertext, 0, passwordCiphertext.Length);
            Buffer.BlockCopy(passwordBytes, passwordIv.Length + passwordCiphertext.Length, passwordTag, 0, passwordTag.Length);
            
            entry.IVPassword = Convert.ToBase64String(passwordIv);
            entry.CypherPassword = Convert.ToBase64String(passwordCiphertext);
            entry.TagPasswords = Convert.ToBase64String(passwordTag);
        }

        entry.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("UpdateEntry called for {EntryId} (Not implemented)", entry.Identifier);
        return true;
    }
}
