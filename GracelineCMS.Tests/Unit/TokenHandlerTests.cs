using GracelineCMS.Domain.Auth;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;

namespace GracelineCMS.Tests.Unit
{
    public class TokenHandlerTests
    {
        [Test]
        public void CanCreateTokenFromEmail()
        {
            var email = "test@email.com";
            ITokenHandler token = new AppTokenHandler("a really good encryption key that will suffice for encryption");
            var jwtToken = token.CreateToken(email);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Assert.That(handler.CanReadToken(jwtToken), Is.True);
        }

        [Test]
        public void CanVerifyToken()
        {
            var email = "test@email.com";
            var securityKey = ";lkjasd;flkjasd;lfkja;lsdfjladkjsfljiuihoiunrinrivhauidhfliasdbhfliauhefi";
            ITokenHandler token = new AppTokenHandler(securityKey);
            var jwtToken = token.CreateToken(email);
            var claimsDict = token.ValidateToken(jwtToken);
            Assert.That(claimsDict.Count, Is.GreaterThan(0));
        }

        [Test]
        public void TokenSignedWithDifferentSecretIsInvalid()
        {
            var email = "test@email.com";
            var securityKey = ";lkjasd;flkjasd;lfkja;lsdfjladkjsfljiuihoiunrinrivhauidhfliasdbhfliauhefi";
            ITokenHandler token = new AppTokenHandler(securityKey);
            var jwtToken = token.CreateToken(email);

            token = new AppTokenHandler("some different secret that should not validate the token");
            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() => token.ValidateToken(jwtToken));
        }
    }
}
