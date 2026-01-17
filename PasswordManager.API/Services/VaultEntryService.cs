using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PasswordManager.API;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.VaultEntries.Requests;
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

        // Ajout du log si présent
        if (!string.IsNullOrEmpty(request.EncryptedLog))
        {
            var log = new VaultLog
            {
                VaultIdentifier = request.VaultIdentifier,
                EncryptedData = request.EncryptedLog,
                Date = DateTime.UtcNow
            };
            _context.VaultLogs.Add(log);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Vault entry {EntryId} created successfully in Vault {VaultId}", entry.Identifier, request.VaultIdentifier);
        return entry;
    }
    
    public async Task<string?> GetEntryPasswordAsync(GetVaultEntryPasswordRequest request)
    {
        var entry = await _context.VaultEntries.FindAsync(request.EntryIdentifier);
        if (entry == null)
        {
            _logger.LogWarning("GetEntryPassword failed: Entry {EntryId} not found", request.EntryIdentifier);
            return null;
        }

        // Ajout du log si présent
        if (!string.IsNullOrEmpty(request.EncryptedLog))
        {
            var log = new VaultLog
            {
                VaultIdentifier = entry.VaultIdentifier,
                EncryptedData = request.EncryptedLog,
                Date = DateTime.UtcNow
            };
            _context.VaultLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        var ivBytes = Convert.FromBase64String(entry.IVPassword);
        var cypherBytes = Convert.FromBase64String(entry.CypherPassword);
        var tagBytes = Convert.FromBase64String(entry.TagPasswords);

        var combinedBytes = new byte[ivBytes.Length + cypherBytes.Length + tagBytes.Length];
        Buffer.BlockCopy(ivBytes, 0, combinedBytes, 0, ivBytes.Length);
        Buffer.BlockCopy(cypherBytes, 0, combinedBytes, ivBytes.Length, cypherBytes.Length);
        Buffer.BlockCopy(tagBytes, 0, combinedBytes, ivBytes.Length + cypherBytes.Length, tagBytes.Length);

        _logger.LogInformation("Password retrieved for entry {EntryId}", request.EntryIdentifier);
        return Convert.ToBase64String(combinedBytes);
    }
    
    // Surcharge pour compatibilité si nécessaire (mais on va essayer de tout migrer)
    public async Task<string?> GetEntryPasswordAsync(Guid entryId)
    {
        return await GetEntryPasswordAsync(new GetVaultEntryPasswordRequest { EntryIdentifier = entryId });
    }

    public async Task<bool> DeleteEntryAsync(DeleteVaultEntryRequest request)
    {
        var entry = await _context.VaultEntries.FindAsync(request.EntryIdentifier);
        if (entry == null)
        {
            _logger.LogWarning("DeleteEntry failed: Entry {EntryId} not found", request.EntryIdentifier);
            return false;
        }
        
        // On récupère le VaultIdentifier avant de supprimer l'entrée pour pouvoir lier le log
        var vaultId = entry.VaultIdentifier;
        
        _context.VaultEntries.Remove(entry);
        
        // Ajout du log si présent
        if (!string.IsNullOrEmpty(request.EncryptedLog))
        {
            var log = new VaultLog
            {
                VaultIdentifier = vaultId,
                EncryptedData = request.EncryptedLog,
                Date = DateTime.UtcNow
            };
            _context.VaultLogs.Add(log);
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Vault entry {EntryId} deleted", request.EntryIdentifier);
        return true;
    }
    
    // Surcharge pour compatibilité si nécessaire, ou à supprimer si on migre tout
    public async Task<bool> DeleteEntryAsync(Guid id)
    {
        return await DeleteEntryAsync(new DeleteVaultEntryRequest { EntryIdentifier = id });
    }
    
    public async Task<bool> UpdateEntryAsync(
        Guid entryId,
        string encryptedData,
        string? encryptedPassword,
        string? encryptedLog = null) // Ajout du paramètre optionnel pour le log
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

        // Ajout du log si présent
        if (!string.IsNullOrEmpty(encryptedLog))
        {
            var log = new VaultLog
            {
                VaultIdentifier = entry.VaultIdentifier,
                EncryptedData = encryptedLog,
                Date = DateTime.UtcNow
            };
            _context.VaultLogs.Add(log);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("UpdateEntry called for {EntryId}", entry.Identifier);
        return true;
    }
}
