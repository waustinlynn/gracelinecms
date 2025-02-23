using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Infrastructure.Auth
{
    public class AuthenticationCode(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IAuthenticationCode
    {
        public async Task<string> CreateAuthCodeAsync(string email)
        {
            var authCode = new AuthCode()
            {
                EmailAddress = email,
                Code = "123456",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            };
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                context.AuthCodes.Add(authCode);
                await context.SaveChangesAsync();
            }
            return authCode.Code;
        }
    }
}
