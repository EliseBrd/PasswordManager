using Microsoft.AspNetCore.Mvc;

using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;

namespace PasswordManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaultEntryController : ControllerBase
    {
        private readonly IVaultEntryService _vaultEntryService;
        private readonly IPermissionService _permissionService;

        public VaultEntryController(IVaultEntryService vaultEntryService, IPermissionService permissionService)
        {
            _vaultEntryService = vaultEntryService;
            _permissionService = permissionService;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateVaultEntry([FromBody] CreateVaultEntryRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) return Unauthorized("User not found or session is invalid.");

            // Vérifie si l'utilisateur a le droit d'ajouter une entrée dans ce coffre
            if (!await _permissionService.CanAccessVaultAsync(currentUser.Identifier, request.VaultIdentifier))
            {
                return Forbid("You do not have access to this vault.");
            }

            var createdEntry = await _vaultEntryService.CreateEntryAsync(request, currentUser.Identifier);

            return Ok(createdEntry.Identifier);
        }
        
        
        // PUT /api/vaultentries/{id} : Modifier une entrée dans un coffre
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(Guid id, [FromBody] VaultEntry entry)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) return Unauthorized("User not found or session is invalid.");

            // Vérifie si l'utilisateur a le droit de modifier cette entrée
            if (!await _permissionService.CanManageVaultEntryAsync(currentUser.Identifier, id))
            {
                return Forbid("You are not authorized to modify this entry.");
            }

            var success = await _vaultEntryService.UpdateEntryAsync(entry);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/vaultentries/{id} : Supprimer une entrée dans un coffre
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(Guid id)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) return Unauthorized("User not found or session is invalid.");

            // Vérifie si l'utilisateur a le droit de supprimer cette entrée
            if (!await _permissionService.CanManageVaultEntryAsync(currentUser.Identifier, id))
            {
                return Forbid("You are not authorized to delete this entry.");
            }

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
        
        // GET /api/vaultEntry/{id}/password : Récupérer le mot de passe d'une entrée dans un coffre
        [HttpGet("{id}/password")]
        public async Task<IActionResult> GetVaultEntryPassword(Guid id)
        {
            
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) 
            {
                return Unauthorized("User not found or session is invalid.");
            }

            // Vérifie si l'utilisateur a le droit de voir cette entrée
            if (!await _permissionService.CanAccessVaultEntryAsync(currentUser.Identifier, id))
            {
                return Forbid("You do not have access to this entry.");
            }

            var encryptedPassword = await _vaultEntryService.GetEntryPasswordAsync(id);
            if (encryptedPassword == null) 
            {
                return NotFound();
            }
            
            Console.WriteLine("[API DEBUG] Password found and returned");
            return Ok(new { encryptedPassword });
        }
    }
}
