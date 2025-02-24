using GracelineCMS.Domain.Auth;
using GracelineCMS.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GracelineCMS.Controllers
{
    [Route("authentication/code")]
    [ApiController]
    public class AuthenticationCodeController(IAuthenticationCode authenticationCode) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAuthCode([FromBody] AuthCodeRequest request)
        {
            await authenticationCode.CreateAuthCodeAsync(request.EmailAddress);
            return Created();
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateAuthCode([FromBody] AuthCodeValidationRequest request)
        {
            var validationResult = await authenticationCode.ValidateCodeWithEmail(request.EmailAddress, request.AuthCode);
            var authCodeValidationResponse = new AuthCodeValidationResponse()
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token"
            };
            return validationResult ? Ok(authCodeValidationResponse) : BadRequest();
        }

    }
}
