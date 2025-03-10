using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Auth;
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
            _user = GlobalFixtures.GetSavedUser();
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
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
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
            var authCodeValidationResponse = JsonConvert.DeserializeObject<AccessRefreshToken>(content);
            Assert.That(authCodeValidationResponse, Is.Not.Null);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Assert.That(handler.CanReadToken(authCodeValidationResponse?.AccessToken), Is.True);

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.RefreshTokens)
                    .FirstAsync();
                Assert.That(user.RefreshTokens.First().RefreshTokenValue, Is.EqualTo(authCodeValidationResponse?.RefreshToken));
            }
        }

        [Test]
        public async Task VerifyingRefreshingTokenWhereRefreshTokenDoesNotExistReturnsBadRequest()
        {
            var refreshTokenRequest = new RefreshTokenRequest
            {
                EmailAddress = _user.EmailAddress,
                RefreshToken = "badrefreshtoken"
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/refresh", refreshTokenRequest);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task RefreshingTokenWhenTokenIsExpiredReturnsBadRequest()
        {
            var email = _user.EmailAddress;
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var authCode = string.Empty;

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
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
            var authCodeValidationResponse = JsonConvert.DeserializeObject<AccessRefreshToken>(content);
            Assert.That(authCodeValidationResponse, Is.Not.Null);

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.RefreshTokens)
                    .FirstAsync();
                user.RefreshTokens.First().ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
                await context.SaveChangesAsync();
            }

            var refreshTokenRequest = new RefreshTokenRequest
            {
                EmailAddress = _user.EmailAddress,
                RefreshToken = authCodeValidationResponse?.RefreshToken!
            };
            var refreshResponse = await GlobalFixtures.PostAsync($"/authentication/refresh", refreshTokenRequest);
            Assert.That(refreshResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task RefreshingTokenWhenTokenIsValidReturnsNewAccessTokenAndRefreshToken()
        {
            var email = _user.EmailAddress;
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);

            var authCode = string.Empty;
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
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
            var firstValidationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);

            var firstAccessRefreshToken = JsonConvert.DeserializeObject<AccessRefreshToken>(await firstValidationResponse.Content.ReadAsStringAsync());

            var refreshTokenRequest = new RefreshTokenRequest
            {
                EmailAddress = _user.EmailAddress,
                RefreshToken = firstAccessRefreshToken?.RefreshToken!
            };
            var refreshResponse = await GlobalFixtures.PostAsync($"/authentication/refresh", refreshTokenRequest);
            var secondAccessRefreshToken = JsonConvert.DeserializeObject<AccessRefreshToken>(await refreshResponse.Content.ReadAsStringAsync());

            Assert.That(secondAccessRefreshToken?.RefreshToken, Is.Not.EqualTo(firstAccessRefreshToken?.RefreshToken));

            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var user = await context.Users
                    .Where(m => m.EmailAddress == email.ToLower())
                    .Include(m => m.RefreshTokens)
                    .FirstAsync();
                Assert.That(user.RefreshTokens.Count, Is.EqualTo(1));
                Assert.That(user.RefreshTokens.First().RefreshTokenValue, Is.EqualTo(secondAccessRefreshToken?.RefreshToken));
            }
        }
    }
}
