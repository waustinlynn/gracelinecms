using GracelineCMS.Domain.Auth;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration
{
    public class AuthenticationCodeTests
    {
#pragma warning disable CS8618
        IAuthenticationCode _authenticationCode;
        IDbContextFactory<AppDbContext> _dbContextFactory;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _dbContextFactory = GlobalFixtures.DbContextFactory;
            _authenticationCode = new AuthenticationCode(_dbContextFactory);
        }
        [Test]
        public async Task CanCreateAuthCodeA()
        {
            var email = "test@email.com";
            await _authenticationCode.CreateAuthCodeAsync(email);


            using (var context = await _dbContextFactory.CreateDbContextAsync())
            {
                var authCode = await context.AuthCodes.Where(m => m.EmailAddress == email).FirstOrDefaultAsync();
                Assert.That(authCode, Is.Not.Null);
                Assert.That(authCode?.EmailAddress, Is.EqualTo(email));
            }
        }

        [Test]
        public async Task CanDeleteAuthCodesThatAreExpired()
        {
            var email = "test@email.com";
            await _authenticationCode.CreateAuthCodeAsync(email);
            using (var context = await _dbContextFactory.CreateDbContextAsync())
            {
                var authCode = await context.AuthCodes.Where(m => m.EmailAddress == email).FirstAsync();
                authCode.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                await context.SaveChangesAsync();

            }
            await _authenticationCode.DeleteExpiredCodes();

            using (var context = await _dbContextFactory.CreateDbContextAsync())
            {
                var authCodes = await context.AuthCodes.Where(m => m.EmailAddress == email).CountAsync();
                Assert.That(authCodes, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ValidateCodeWithEmailReturnsFalse()
        {
            var email = "test@email.com";
            await _authenticationCode.CreateAuthCodeAsync(email);
            var isValid = await _authenticationCode.ValidateCodeWithEmail(email, "badcode");
            Assert.That(isValid, Is.False);
        }

        [Test]
        public async Task ValidateCodeWithEmailReturnsTrue()
        {
            var email = "test@email.com";
            var code = await _authenticationCode.CreateAuthCodeAsync(email);
            var isValid = await _authenticationCode.ValidateCodeWithEmail(email, code);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public async Task ValidateCodeGenerationIsUnique()
        {
            var email1 = "test1@email.com";
            var email2 = "test2@email.com";
            var code1 = await _authenticationCode.CreateAuthCodeAsync(email1);
            var code2 = await _authenticationCode.CreateAuthCodeAsync(email2);
            Assert.That(code1, Is.Not.EqualTo(code2));
        }
    }
}
