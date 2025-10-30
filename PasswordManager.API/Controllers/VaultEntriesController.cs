using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault;

namespace PasswordManager.API.Controllers
{
    // Controller exposant les endpoints REST pour les entrées de coffre
    // Il ne contient pas de logique métier, juste la liaison entre HTTP et le service

    [ApiController]
    [Route("api/[controller]")]
    public class VaultEntriesController : ControllerBase
    {
        private readonly IVaultEntryService _vaultEntryService;

        public VaultEntriesController(IVaultEntryService vaultEntryService)
        {
            _vaultEntryService = vaultEntryService;
        }

        // GET /api/vaultentries/vault/{vaultId} : Récupérer toutes les entrées d’un coffre par son identifiant (VaultIdentifier)
        [HttpGet("vault/{vaultId}")]
        public async Task<IActionResult> GetEntriesByVaultId(Guid vaultId)
        {
            var entries = await _vaultEntryService.GetEntriesByVaultIdAsync(vaultId);
            return Ok(entries);
        }

        // GET /api/vaultentries/{id} : Recherche une entrée de coffre par son identifiant unique (GUID)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntryById(Guid id)
        {
            var entry = await _vaultEntryService.GetByIdAsync(id);

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        // POST /api/vaultentries : Créer une nouvelle entrée dans un coffre
        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] VaultEntry entry)
        {
            if (entry == null)
                return BadRequest("Invalid entry.");

            var created = await _vaultEntryService.CreateEntryAsync(entry);
            return CreatedAtAction(nameof(GetEntriesByVaultId),
                new { vaultId = created.VaultIdentifier }, created);
        }

        // PUT /api/vaultentries/{id} : Modifier une entrée dans un coffre
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(Guid id, [FromBody] VaultEntry entry)
        {
            if (id != entry.Identifier)
                return BadRequest("ID mismatch.");

            var success = await _vaultEntryService.UpdateEntryAsync(entry);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/vaultentries/{id} : Supprimer une entrée dans un coffre
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(Guid id)
        {
            var success = await _vaultEntryService.DeleteEntryAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

    }
}
