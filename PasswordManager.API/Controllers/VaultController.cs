using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.API.Controllers
{
    
     [Authorize]
     [ApiController]
     [Route("[controller]")]
    public class VaultController :ControllerBase
    {
        [HttpPost] 
        public IActionResult createVault()
        {
            return Ok(new { message = "New vault created" });
        }

       
        
        [HttpGet]
        public IActionResult getVault()
        {
            return Ok(new { message = "Vault list of entry" });
        }
        
        [HttpDelete]
        public IActionResult deleteVault()
        {
            return Ok(new { message = "Vault deleted" });
        }
    }

    [Authorize]
    [ApiController]
    [Route("/vaults")]
    public class vaultsController : ControllerBase
    {
        [HttpGet]
        public IActionResult getVaults()
        {
            return Ok(new { message = "List off accessible vaults" });
        }
    }
}
