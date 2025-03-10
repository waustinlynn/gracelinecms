using GracelineCMS.Domain.Auth;
using GracelineCMS.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GracelineCMS.Controllers
{
    [Route("authentication")]
    [ApiController]
    public class AuthenticationController(ITokenHandler tokenHandler) : ControllerBase
    {
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            return Ok(await tokenHandler.RefreshToken(request.EmailAddress, request.RefreshToken));
        }
    }
}
