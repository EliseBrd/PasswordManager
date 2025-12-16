using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class VaultEntryController : ControllerBase
    {
        [HttpPost]
        public IActionResult createVaultEntry()
        {
            return Ok(new { message = "New vault entry created" });
        }

        [HttpPatch]
        public IActionResult updateVaultEntry()
        {
            return Ok(new { message = "Vault entry updated" });
        }
        
        [HttpGet]
        public IActionResult getVaultEntry()
        {
            return Ok(new { message = "Vault entry list" });
        }

        [HttpDelete]
        public IActionResult deleteVaultEntry()
        {
            return Ok(new { message = "Vault entry deleted" }); 
        }
    


    }
} 