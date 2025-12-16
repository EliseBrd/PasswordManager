using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault;

namespace PasswordManager.API.Controllers
{
    // Contrôleur Web API pour les opérations sur les Vaults
    // Il appelle le service, et ne fait PAS de logique métier directement

    [ApiController]
    [Route("api/[controller]")]
    public class VaultsController : ControllerBase
    {
        private readonly IVaultService _vaultService;

        public VaultsController(IVaultService vaultService)
        {
            _vaultService = vaultService;
        }

        // GET /api/vaults : Récupérer tous les coffres
        [HttpGet]
        public async Task<IActionResult> GetVaults()
        {
            var vaults = await _vaultService.GetAllVaultsAsync();
            return Ok(vaults);
        }

        // GET /api/vaults/{id} : Récupérer un coffre par son identifiant
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVault(Guid id)
        {
            var vault = await _vaultService.GetVaultByIdAsync(id);
            if (vault == null)
                return NotFound();

            return Ok(vault);
        }

        // POST /api/vaults : Créer un nouveau coffre
        [HttpPost]
        public async Task<IActionResult> CreateVault([FromBody] VaultCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Invalid vault data.");

            var creatorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var vault = await _vaultService.CreateVaultAsync(dto.Name, dto.MasterSalt, dto.Salt, creatorId);

            return CreatedAtAction(nameof(GetVault), new { id = vault.Identifier }, vault);
        }

        // UPDATE /api/vaults : Modifier un coffre par son identifiant
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVault(Guid id, [FromBody] Vault vault)
        {
            //if (id != vault.Identifier)
            //    return BadRequest("ID mismatch.");
//
            var updated = await _vaultService.UpdateVaultAsync(vault);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/vaults : Supprimer un coffre par son identifiant
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVault(Guid id)
        {
            var deleted = await _vaultService.DeleteVaultAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
