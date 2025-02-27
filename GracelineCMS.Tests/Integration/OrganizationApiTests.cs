using GracelineCMS.Infrastructure.Organization;
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
            var organizationRequest = new CreateOrganizationRequest()
            {
                Name = "Test Organization"
            };
            var authToken = GlobalFixtures.GetAuthToken(GlobalFixtures.GlobalAdminEmail);
            var response = await GlobalFixtures.PostAsync($"/organization", organizationRequest, authToken);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
