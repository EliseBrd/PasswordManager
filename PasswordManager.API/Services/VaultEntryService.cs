using PasswordManager.API;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;

namespace PasswordManager.API.Services;
public class VaultEntryService : IVaultEntryService
{
    private readonly PasswordManagerDBContext _context;

    public VaultEntryService(PasswordManagerDBContext context)
    {
        _context = context;
    }

    public async Task<VaultEntry> CreateEntryAsync(CreateVaultEntryRequest request, Guid creatorId)
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

        return entry;
    }
    
    public async Task<string?> GetEntryPasswordAsync(Guid entryId)
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

    public async Task<bool> DeleteEntryAsync(Guid id)
    {
        var entry = await _context.VaultEntries.FindAsync(id);
        if (entry == null) return false;
        _context.VaultEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateEntryAsync(VaultEntry entry)
    {
        return true;
    }
}
