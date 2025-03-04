using Bogus.DataSets;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Content;
using GracelineCMS.Infrastructure.Repository;
using GracelineCMS.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Tests.Integration.Api
{
    public class ContentModuleApiTests
    {
        private User _user;
        private Organization _organization;
        [SetUp]
        public void Setup()
        {
            using (var context = GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
            {
                _user = FakeUser.User;
                _organization = FakeOrganization.Organization;
                _organization.Users.Add(_user);
                context.Organizations.Add(_organization);
                context.SaveChanges();
            }
        }

        [Test]
        public async Task CannotCreateContentModuleIfMissingOrganization()
        {
            var organizationId = Guid.NewGuid().ToString();
            var createContentModuleRequest = new ContentModuleRequest()
            {
                OrganizationId = organizationId,
                Name = "Test Content Module",
                Description = "This is a test content module"
            };
            var authToken = GlobalFixtures.GetAuthToken(_user.EmailAddress);
            var headers = new Dictionary<string, string>
            {
                {"OrganizationId", _organization.Id.ToString() }
            };
            var response = await GlobalFixtures.PostAsync("/contentmodule", createContentModuleRequest, authToken, headers);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task CanCreateContentModule()
        {
            var createContentModuleRequest = new ContentModuleRequest()
            {
                OrganizationId = _organization.Id,
                Name = "Test Content Module",
                Description = "This is a test content module"
            };
            var authToken = GlobalFixtures.GetAuthToken(_user.EmailAddress);
            var headers = new Dictionary<string, string>
            {
                {"OrganizationId", _organization.Id.ToString() }
            };
            var response = await GlobalFixtures.PostAsync("/contentmodule", createContentModuleRequest, authToken, headers);
            Assert.That(response.IsSuccessStatusCode);
            using (var context = await GlobalFixtures.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                var contentModule = await context.ContentModules.Include(c => c.Organization).FirstOrDefaultAsync(m => m.Name == createContentModuleRequest.Name);
                Assert.That(contentModule, Is.Not.Null);
                Assert.That(contentModule?.Organization.Id, Is.EqualTo(_organization.Id));
                Assert.That(contentModule?.Description, Is.EqualTo(createContentModuleRequest.Description));
            }
        }
        [Test]
        public void Reminder()
        {
            //Refactor tests to provide User, Organization, AuthToken, and Headers from GlobalFixtures
            Assert.That(false);
        }
    }
}
