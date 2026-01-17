using Microsoft.AspNetCore.Mvc;

using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.VaultEntries.Requests;
using PasswordManager.Dto.VaultsEntries.Requests;

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
                return StatusCode(403, "You do not have access to this vault.");
            }

            var createdEntry = await _vaultEntryService.CreateEntryAsync(request, currentUser.Identifier);

            return Ok(createdEntry.Identifier);
        }
        
        
        // PUT /api/vaultentries/{id} : Modifier une entrée dans un coffre
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(Guid id, [FromBody] UpdateVaultEntryRequest  request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) return Unauthorized("User not found or session is invalid.");

            // Vérifie si l'utilisateur a le droit de modifier cette entrée
            if (!await _permissionService.CanManageVaultEntryAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You are not authorized to modify this entry.");
            }

            var success = await _vaultEntryService.UpdateEntryAsync(
                request.EntryIdentifier,
                request.EncryptedData,
                request.EncryptedPassword,
                request.EncryptedLog); // Passage du log chiffré

            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/vaultentries/{id} : Supprimer une entrée dans un coffre
        // Note: Utilisation de [FromBody] pour passer le log chiffré. 
        // Certains clients HTTP ou proxies peuvent bloquer les body sur DELETE.
        // Si problème, passer en POST /delete ou utiliser un header.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(Guid id, [FromBody] DeleteVaultEntryRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) return Unauthorized("User not found or session is invalid.");

            // Vérifie si l'utilisateur a le droit de supprimer cette entrée
            if (!await _permissionService.CanManageVaultEntryAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You are not authorized to delete this entry.");
            }

            // Sécurité : on s'assure que l'ID de l'URL correspond à l'ID du body
            if (id != request.EntryIdentifier)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var success = await _vaultEntryService.DeleteEntryAsync(request);
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
        
        // POST /api/vaultEntry/{id}/password/access : Récupérer le mot de passe d'une entrée avec log d'audit
        [HttpPost("{id}/password/access")]
        public async Task<IActionResult> AccessVaultEntryPassword(Guid id, [FromBody] GetVaultEntryPasswordRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) 
            {
                return Unauthorized("User not found or session is invalid.");
            }

            // Vérifie si l'utilisateur a le droit de voir cette entrée
            if (!await _permissionService.CanAccessVaultEntryAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You do not have access to this entry.");
            }
            
            // Sécurité : on s'assure que l'ID de l'URL correspond à l'ID du body
            if (id != request.EntryIdentifier)
            {
                return BadRequest("ID mismatch");
            }

            var encryptedPassword = await _vaultEntryService.GetEntryPasswordAsync(request);
            if (encryptedPassword == null) 
            {
                return NotFound();
            }
            
            return Ok(new { encryptedPassword });
        }
    }
}
