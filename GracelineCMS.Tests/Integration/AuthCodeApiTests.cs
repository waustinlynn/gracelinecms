using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration
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
        public async Task CanCallApiToCreateAuthCode()
        {
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = _user.EmailAddress
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
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
                    .Where(m => m.EmailAddress == email)
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
        }
    }
}
