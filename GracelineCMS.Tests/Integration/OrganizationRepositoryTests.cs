using GracelineCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GracelineCMS.Tests.Integration
{
    public class OrganizationRepositoryTests
    {
        [Test]
        public async Task CanCreateAndRetrieveOrganization()
        {
            using (var context = await GlobalFixtures.DbContextFactory.CreateDbContextAsync())
            {
                var organization = new Organization
                {
                    Name = "Test Organization"
                };

                context.Organizations.Add(organization);
                await context.SaveChangesAsync();
                var retrievedOrganizations = await context.Organizations.ToListAsync();
                Assert.That(retrievedOrganizations.Count, Is.EqualTo(1));
                var retrievedOrganization = retrievedOrganizations.FirstOrDefault();
                Assert.That(retrievedOrganization?.Name, Is.EqualTo(organization.Name));
                Assert.That(retrievedOrganization?.Id, Is.EqualTo(organization.Id));
            }
        }
    }
}
