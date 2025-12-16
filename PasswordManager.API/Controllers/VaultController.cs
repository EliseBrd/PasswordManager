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

        [HttpPost("{id}/access")]
        public async Task<IActionResult> AccessVault(Guid id, [FromBody] AccessVaultRequest request)
        {
            var vault = await _vaultService.AccessVaultAsync(id, request.Password);

            if (vault == null)
            {
                // We return a generic error to avoid indicating whether the vault exists or if the password was wrong.
                return Unauthorized("Access denied. Invalid vault ID or password.");
            }

            // For now, we return the vault data. In a real-world app, you'd likely return a session-specific token.
            return Ok(vault);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVault([FromBody] CreateVaultRequest request)
        {
            // Retrieve the user from the HttpContext
            var currentUser = HttpContext.Items["CurrentUser"] as AppUser;
            if (currentUser == null)
            {
                // This should not happen if the EnsureUserMiddleware is configured correctly
                return Unauthorized("User not found or session is invalid.");
            }

            //Call the service to create the vault
            var createdVault = await _vaultService.CreateVaultAsync(request.Name, request.Password, currentUser.Identifier);

            // 3. Return a 201 Created response
            // The response includes the location of the newly created resource.
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
