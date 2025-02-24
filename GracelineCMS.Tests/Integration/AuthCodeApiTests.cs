using Google.Apis.Auth.OAuth2.Requests;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GracelineCMS.Tests.Integration
{
    public class AuthCodeApiTests
    {
        [Test]
        public async Task CanCallApiToCreateAuthCode()
        {
            var email = "test@email.com";
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
        }

        [Test]
        public async Task VerifyingAuthCodeWithIncorrectEmailReturnsBadRequest()
        {
            var email = "test@email.com";
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var authCodeValidationRequest = new AuthCodeValidationRequest()
            {
                EmailAddress = email,
                AuthCode = "randomcode"
            };
            var validationResponse = await GlobalFixtures.PostAsync($"/authentication/code/validate", authCodeValidationRequest);
            Assert.That(validationResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task VerifyingAuthCodeWithCorrectEmailReturnsBearerAndAccessToken()
        {
            var email = "test@email.com";
            var authCodeRequest = new AuthCodeRequest()
            {
                EmailAddress = email
            };
            var response = await GlobalFixtures.PostAsync($"/authentication/code", authCodeRequest);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            var authCode = string.Empty;
            using(var context = await GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                authCode = await context.AuthCodes.Where(m => m.EmailAddress == email).Select(m => m.Code).FirstAsync();
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
