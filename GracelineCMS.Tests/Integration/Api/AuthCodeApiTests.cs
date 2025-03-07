using GracelineCMS.Domain.Communication;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;

namespace GracelineCMS.Tests.Integration.Api
{
    public class AuthCodeApiTests
    {
#pragma warning disable CS8618
        private User _user;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _user = GlobalFixtures.GetSavedUser(GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>());
        }
        [Test]
        public async Task CanCallApiToCreateAuthCodeAndValidateEmailSent()
        {
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = _user.EmailAddress
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var testEmailClient = GlobalFixtures.GetRequiredService<IEmailClient>() as TestEmailClient;
            Assert.That(testEmailClient?.SentMessages.Any(m => m.ToAddress == _user.EmailAddress), Is.True);
        }

        [Test]
        public async Task VerifyingAuthCodeWithIncorrectEmailReturnsBadRequest()
        {
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = _user.EmailAddress
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var authCodeValidationRequest = new AuthCodeValidationRequest()
            {
                EmailAddress = _user.EmailAddress,
                AuthCode = "randomcode"
            };
            var validationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);
            Assert.That(validationResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task VerifyingAuthCodeWithCorrectEmailReturnsBearerAndAccessToken()
        {
            var email = _user.EmailAddress;
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);


            var authCode = string.Empty;
            using (var context = await GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                authCode = (await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.AuthCodes)
                    .FirstAsync())
                    .AuthCodes.First().Code;

            }


            var authCodeValidationRequest = new AuthCodeValidationRequest()
            {
                EmailAddress = email,
                AuthCode = authCode
            };
            var validationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);
            Assert.That(validationResponse.IsSuccessStatusCode, Is.True);


            var content = await validationResponse.Content.ReadAsStringAsync();
            var authCodeValidationResponse = JsonConvert.DeserializeObject<AuthCodeValidationResponse>(content);
            Assert.That(authCodeValidationResponse, Is.Not.Null);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Assert.That(handler.CanReadToken(authCodeValidationResponse?.AccessToken), Is.True);

            using (var context = await GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                var user = await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.RefreshTokens)
                    .FirstAsync();
                Assert.That(user.RefreshTokens.First(), Is.EqualTo(authCodeValidationResponse?.AccessToken));
            }
        }
    }
}
