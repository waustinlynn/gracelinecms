using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace GracelineCMS.Infrastructure.Auth
{
    public class AuthenticationCode(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IAuthenticationCode
    {
        private static int RandomCode()
        {
            return Random.Shared.Next(100000, 999999);
        }
        public async Task<string> CreateAuthCodeAsync(string email)
        {
            Random.Shared.Next(100000, 999999).ToString();
            var authCode = new AuthCode()
            {
                EmailAddress = email,
                Code = RandomCode().ToString(),
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

        public async Task DeleteExpiredCodes()
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var expiredCodes = await context.AuthCodes
                    .Where(m => m.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();
                context.AuthCodes.RemoveRange(expiredCodes);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidateCodeWithEmail(string email, string code)
        {
            using (var context = await dbContextFactory.CreateDbContextAsync())
            {
                var authCode = context.AuthCodes
                    .Where(m => m.EmailAddress == email && m.Code == code)
                    .FirstOrDefault();
                if (authCode == null)
                {
                    return false;
                }
                if (authCode.ExpiresAt < DateTime.UtcNow)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
