using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Tests.Integration
{
    public class AuthenticationCodeTests
    {
        [Test]
        public async Task CanCreateAuthCodeA()
        {
            var email = "test@email.com";
            var dbContextFactory = GlobalFixtures.DbContextFactory;
            IAuthenticationCode authenticationCode = new AuthenticationCode(dbContextFactory);
            await authenticationCode.CreateAuthCodeAsync(email);


            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var authCode = await context.AuthCodes.Where(m => m.EmailAddress == email).FirstOrDefaultAsync();
                Assert.That(authCode, Is.Not.Null);
                Assert.That(authCode?.EmailAddress, Is.EqualTo(email));
            }
        }
    }
}
