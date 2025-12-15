using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace PasswordManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetClaims()
        {
            var claims = User.Claims
                .Select(c => new { c.Type, c.Value })
                .ToList();

            return Ok(claims);
        }
    }
}