using GracelineCMS.Domain.Auth;
using GracelineCMS.Infrastructure.Auth;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;

namespace GracelineCMS.Tests.Unit
{
    public class TokenHandlerTests
    {

#pragma warning disable CS8618
        private IClaimsProvider _claimsProvider;
        private ITokenHandler _tokenHandler;
        private string _secretKey;
        private string _email;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _email = "test@email.com";
            _claimsProvider = new ClaimsProvider("test@email.com");
            _secretKey = "a really good encryption key that will suffice for encryption";
            _tokenHandler = new AppTokenHandler(_claimsProvider, _secretKey);
        }

        [Test]
        public void CanCreateTokenFromEmail()
        {
            var jwtToken = _tokenHandler.CreateToken(_email);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Assert.That(handler.CanReadToken(jwtToken), Is.True);
        }
    }
}
