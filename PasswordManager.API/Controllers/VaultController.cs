using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;
using PasswordManager.Dto.User;
using System;
using System.Linq;

namespace PasswordManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VaultController : ControllerBase
    {
        private readonly IVaultService _vaultService;
        private readonly IPermissionService _permissionService;

        public VaultController(IVaultService vaultService, IPermissionService permissionService)
        {
            _vaultService = vaultService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccessibleVaults([FromQuery] bool? isShared = null)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            var vaults = await _vaultService.GetAccessibleVaultsAsync(currentUser.Identifier);
            
            if (isShared.HasValue)
            {
                vaults = vaults.Where(v => v.IsShared == isShared.Value);
            }

            return Ok(vaults);
        }

        [HttpPost("{id}/access")]
        public async Task<IActionResult> AccessVault(Guid id, [FromBody] AccessVaultRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) return Unauthorized("User not found or session is invalid.");

            if (!await _permissionService.CanAccessVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You do not have access to this vault.");
            }

            var vault = await _vaultService.AccessVaultAsync(id, request.Password);

            if (vault == null)
            {
                return Unauthorized("Access denied. Invalid vault ID or password.");
            }
            
            return Ok(vault);
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> UnlockVault(Guid id, [FromBody] AccessVaultRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null) return Unauthorized("User not found or session is invalid.");

            if (!await _permissionService.CanAccessVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You do not have access to this vault.");
            }
            
            var vault = await _vaultService.AccessVaultAsync(id, request.Password);

            if (vault == null)
            {
                return Unauthorized("Invalid password.");
            }

            var response = new VaultUnlockResponse
            {
                MasterSalt = vault.MasterSalt,
                EncryptedKey = vault.EncryptKey,
                Entries = vault.Entries.Select(e => {
                    var ivBytes = Convert.FromBase64String(e.IVData);
                    var cypherBytes = Convert.FromBase64String(e.CypherData);
                    var tagBytes = Convert.FromBase64String(e.TagData);

                    var combinedBytes = new byte[ivBytes.Length + cypherBytes.Length + tagBytes.Length];
                    Buffer.BlockCopy(ivBytes, 0, combinedBytes, 0, ivBytes.Length);
                    Buffer.BlockCopy(cypherBytes, 0, combinedBytes, ivBytes.Length, cypherBytes.Length);
                    Buffer.BlockCopy(tagBytes, 0, combinedBytes, ivBytes.Length + cypherBytes.Length, tagBytes.Length);
                    
                    return new VaultUnlockEntryDto
                    {
                        Identifier = e.Identifier,
                        EncryptedData = Convert.ToBase64String(combinedBytes)
                    };
                }).ToList()
            };
            
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVault([FromBody] CreateVaultRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            var createdVault = await _vaultService.CreateVaultAsync(request, currentUser.Identifier);

            return CreatedAtAction(nameof(GetVaultById), new { id = createdVault.Identifier }, createdVault);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaultById(Guid id)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            if (!await _permissionService.CanAccessVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You do not have access to this vault.");
            }

            var vault = await _vaultService.GetVaultByIdAsync(id);
            if (vault == null)
            {
                return NotFound();
            }

            var response = new VaultDetailsResponse
            {
                Identifier = vault.Identifier,
                Name = vault.Name,
                CreatorIdentifier = vault.CreatorIdentifier,
                IsCreator = vault.CreatorIdentifier == currentUser.Identifier,
                IsShared = vault.IsShared,
                SharedWith = vault.UserAccesses
                    .Where(ua => ua.UserIdentifier != currentUser.Identifier) // Optionally exclude current user from list
                    .Select(ua => new VaultUserResponse
                    {
                        Identifier = ua.User.Identifier,
                        Email = ua.User.Email,
                        IsAdmin = ua.IsAdmin
                    }).ToList()
            };
            
            return Ok(response);
        }

        [HttpPut("{id}/sharing")]
        public async Task<IActionResult> UpdateVaultSharing(Guid id, [FromBody] UpdateVaultSharingRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            if (!await _permissionService.CanManageVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You are not authorized to update this vault.");
            }

            var success = await _vaultService.UpdateVaultSharingAsync(id, request.IsShared, currentUser.Identifier);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareVault(Guid id)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            if (!await _permissionService.CanManageVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You are not authorized to share this vault.");
            }

            var success = await _vaultService.ShareVaultAsync(id, currentUser.Identifier);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("{id}/users")]
        public async Task<IActionResult> AddUserToVault(Guid id, [FromBody] AddUserToVaultRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            if (!await _permissionService.CanManageVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You are not authorized to add users to this vault.");
            }

            var success = await _vaultService.AddUserToVaultAsync(id, request.UserId, request.IsAdmin, currentUser.Identifier);
            if (!success)
            {
                return BadRequest("Could not add user to vault. Check if user exists and vault is shared.");
            }

            return NoContent();
        }

        [HttpDelete("{id}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromVault(Guid id, Guid userId)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            if (!await _permissionService.CanManageVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You are not authorized to remove users from this vault.");
            }

            var success = await _vaultService.RemoveUserFromVaultAsync(id, userId, currentUser.Identifier);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut("{id}/users/{userId}")]
        public async Task<IActionResult> UpdateUserAccess(Guid id, Guid userId, [FromBody] UpdateUserAccessRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            if (!await _permissionService.CanManageVaultAsync(currentUser.Identifier, id))
            {
                return StatusCode(403, "You are not authorized to manage users in this vault.");
            }

            var success = await _vaultService.UpdateUserAccessAsync(id, userId, request.IsAdmin, currentUser.Identifier);
            if (!success)
            {
                return BadRequest("Could not update user access.");
            }

            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVault(Guid id)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
                return Unauthorized("User not found or session is invalid.");

            if (!await _permissionService.CanManageVaultAsync(currentUser.Identifier, id))
                return StatusCode(403, "You are not authorized to delete this vault.");

            var success = await _vaultService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVault(Guid id, [FromBody] UpdateVaultRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
                return Unauthorized();

            if (!await _permissionService.CanManageVaultAsync(currentUser.Identifier, id))
                return StatusCode(403, "Not authorized");

            var success = await _vaultService.UpdateVaultAsync(id, request, currentUser.Identifier);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
