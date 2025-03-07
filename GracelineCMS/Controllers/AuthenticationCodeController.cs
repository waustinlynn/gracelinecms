using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GracelineCMS.Controllers
{
    [Route("authentication/code")]
    [ApiController]
    public class AuthenticationCodeController(
        IAuthenticationCodeEmail authenticationCodeEmail,
        IAuthenticationCode authenticationCode,
        ITokenHandler tokenHandler
    ) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAuthCode([FromBody] AuthCodeRequest request)
        {
            await authenticationCodeEmail.GetCodeAndEmailUser(request.EmailAddress);
            return Created();
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateAuthCode([FromBody] AuthCodeValidationRequest request)
        {
            var validationResult = await authenticationCode.ValidateCodeWithEmail(request.EmailAddress, request.AuthCode);
            var authCodeValidationResponse = new AuthCodeValidationResponse()
            {
                AccessToken = tokenHandler.CreateToken(request.EmailAddress),
                RefreshToken = "refresh_token"
            };
            return validationResult ? Ok(authCodeValidationResponse) : BadRequest();
        }
    }
}
