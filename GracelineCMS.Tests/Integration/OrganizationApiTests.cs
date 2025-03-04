using GracelineCMS.Infrastructure.Organization;
using GracelineCMS.Infrastructure.Repository;
using GracelineCMS.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Net;

namespace GracelineCMS.Tests.Integration
{
    public class OrganizationApiTests
    {
        [Test]
        public async Task CannotCreateOrganizationIfNotAGlobalAdmin()
        {
            var organizationRequest = new CreateOrganizationRequest()
            {
                Name = "Test Organization"
            };
            var authToken = GlobalFixtures.GetAuthToken("notaglobaladmin@email.com");
            var response = await GlobalFixtures.PostAsync($"/organization", organizationRequest, authToken);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task CanCreateOrganizationIfGlobalAdmin()
        {
            var fakeOrganization = FakeOrganization.Organization;
            var organizationRequest = new CreateOrganizationRequest()
            {
                Name = fakeOrganization.Name
            };
            var authToken = GlobalFixtures.GetAuthToken(GlobalFixtures.GlobalAdminEmail);
            var response = await GlobalFixtures.PostAsync($"/organization", organizationRequest, authToken);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using (var context = await GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                var organization = await context.Organizations.FirstOrDefaultAsync(m => m.Name == fakeOrganization.Name);
                Assert.That(organization, Is.Not.Null);
            }
        }
    }
}
