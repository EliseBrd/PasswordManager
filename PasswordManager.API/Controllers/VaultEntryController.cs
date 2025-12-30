using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault;
using PasswordManager.Dto.Vault.Requests;

namespace PasswordManager.API.Controllers
{
    // Controller exposant les endpoints REST pour les entrées de coffre
    // Il ne contient pas de logique métier, juste la liaison entre HTTP et le service

    [ApiController]
    [Route("api/[controller]")]
    public class VaultEntryController : ControllerBase
    {
        private readonly IVaultEntryService _vaultEntryService;

        public VaultEntryController(IVaultEntryService vaultEntryService)
        {
            _vaultEntryService = vaultEntryService;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateVaultEntry([FromBody] CreateVaultEntryRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            var createdEntry = await _vaultEntryService.CreateEntryAsync(request, currentUser.Identifier);

            return Ok(createdEntry.Identifier);
        }
        
        
        // PUT /api/vaultentries/{id} : Modifier une entrée dans un coffre
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(Guid id, [FromBody] VaultEntry entry)
        {
           // if (id != entry.Identifier)
           //     return BadRequest("ID mismatch.");

            var success = await _vaultEntryService.UpdateEntryAsync(entry);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/vaultentries/{id} : Supprimer une entrée dans un coffre
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            try
            {
                var success = await _vaultEntryService.DeleteEntryAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception DELETE VaultEntry {id}: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        
        // GET /api/vaultentries/{id}/password : Récupérer le mot de passe d'une entrée dans un coffre
        [HttpGet("entry/{id}/password")]
        public async Task<IActionResult> GetVaultEntryPassword(int id)
        {
            var encryptedPassword = await _vaultEntryService.GetEntryPasswordAsync(id);
            if (encryptedPassword == null) return NotFound();
            return Ok(new { encryptedPassword });
        }

    }
}
