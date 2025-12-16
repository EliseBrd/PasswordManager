using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.API.Objects;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.Vault.Requests;

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

        [HttpPost]
        public async Task<IActionResult> CreateVault([FromBody] CreateVaultRequest request)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                return Unauthorized("User not found or session is invalid.");
            }

            var createdVault = await _vaultService.CreateVaultAsync(request.Name, request.Password, currentUser.Identifier);

            return CreatedAtAction(nameof(GetVaultById), new { id = new Guid(createdVault.Identifier) }, createdVault);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaultById(Guid id)
        {
            var vault = await _vaultService.GetVaultByIdAsync(id);
            if (vault == null)
            {
                return NotFound();
            }
            return Ok(vault);
        }
    }
}
