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

        public VaultController(IVaultService vaultService)
        {
            _vaultService = vaultService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccessibleVaults()
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            var vaults = await _vaultService.GetAccessibleVaultsAsync(currentUser.Identifier);
            return Ok(vaults);
        }

        [HttpPost("{id}/access")]
        public async Task<IActionResult> AccessVault(Guid id, [FromBody] AccessVaultRequest request)
        {
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
            var vault = await _vaultService.AccessVaultAsync(id, request.Password);

            if (vault == null)
            {
                return Unauthorized("Invalid password.");
            }

            var response = new VaultUnlockResponse
            {
                MasterSalt = vault.MasterSalt,
                EncryptedKey = vault.EncryptKey,
                Entries = vault.Entries.Select(e =>
                {
                    var ivBytes = Convert.FromBase64String(e.IVData);
                    var cypherBytes = Convert.FromBase64String(e.CypherData);
                    var tagBytes = Convert.FromBase64String(e.TagData);

                    var combinedBytes = new byte[
                        ivBytes.Length + cypherBytes.Length + tagBytes.Length
                    ];

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

            return CreatedAtAction(nameof(GetVaultById), new { id = new Guid(createdVault.Identifier) }, createdVault);
        }

        [HttpPost("entry")]
        public async Task<IActionResult> CreateVaultEntry([FromBody] CreateVaultEntryRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            var createdEntry = await _vaultService.CreateVaultEntryAsync(request, currentUser.Identifier);

            return CreatedAtAction(nameof(GetVaultById), new { id = new Guid(createdEntry.VaultIdentifier) }, createdEntry);
        }

        [HttpGet("entry/{id}/password")]
        public async Task<IActionResult> GetVaultEntryPassword(int id)
        {
            var encryptedPassword = await _vaultService.GetVaultEntryPasswordAsync(id);
            if (encryptedPassword == null) return NotFound();
            return Ok(new { encryptedPassword });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaultById(Guid id)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
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
                SharedWith = vault.SharedUsers.Select(u => new UserSummaryResponse
                {
                    Identifier = u.Identifier,
                    Email = u.Email
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

            var success = await _vaultService.UpdateVaultSharingAsync(id, request.IsShared, currentUser.Identifier);
            if (!success)
            {
                return Forbid("You are not authorized to update this vault or the vault does not exist.");
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

            var success = await _vaultService.ShareVaultAsync(id, currentUser.Identifier);
            if (!success)
            {
                return Forbid("You are not authorized to share this vault or the vault does not exist.");
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

            var success = await _vaultService.AddUserToVaultAsync(id, request.UserId, currentUser.Identifier);
            if (!success)
            {
                return Forbid("You are not authorized to add users to this vault, the vault is not shared, or the user to add does not exist.");
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

            var success = await _vaultService.RemoveUserFromVaultAsync(id, userId, currentUser.Identifier);
            if (!success)
            {
                return Forbid("You are not authorized to remove users from this vault, or the user does not exist.");
            }

            return NoContent();
        }
    }
}
