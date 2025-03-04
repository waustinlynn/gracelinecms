using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration.Api
{
    public class OrganizationAuthorizationApiTests
    {
#pragma warning disable CS8618
        private Organization _organization;
        private User _user;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            var dbContextFactory = GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>();
            _user = GlobalFixtures.GetSavedUser(dbContextFactory);
            using (var context = dbContextFactory.CreateDbContext())
            {
                context.Users.Update(_user);
                _organization = new Organization()
                {
                    Name = "Authorized Organization",
                    Users = new List<User>()
                    {
                        _user
                    }
                };
                context.Organizations.Add(_organization);
                context.SaveChanges();
            }
        }

        [Test]
        public async Task CannotGetOrganizationIfUserIsNotAssociated()
        {
            var authToken = GlobalFixtures.GetAuthToken(_user.EmailAddress);
            var response = await GlobalFixtures.GetAsync($"/organization/{_organization.Id}", authToken);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task CanGetOrganizationIfUserIsAssociated()
        {
            var authToken = GlobalFixtures.GetAuthToken(_user.EmailAddress);
            var headers = new Dictionary<string, string>
            {
                { "OrganizationId", _organization.Id.ToString() }
            };
            var response = await GlobalFixtures.GetAsync($"/organization/{_organization.Id}", authToken, headers);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            var organizationResponse = await response.Content.ReadAsStringAsync();
            var organization = JsonConvert.DeserializeObject<Organization>(organizationResponse);
            Assert.That(organization?.Name, Is.EqualTo(_organization.Name));
        }
    }
}
