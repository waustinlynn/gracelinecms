using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace GracelineCMS.Tests
{
    [SetUpFixture]
    public class GlobalFixtures
    {
        private static readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        public static IDbContextFactory<AppDbContext> DbContextFactory
        {
            get
            {
                using (var context = _dbContextFactory.CreateDbContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.Migrate();
                }
                return _dbContextFactory;
            }
        }
        public static HttpClient HttpClient { get; private set; }
        private const string ConnectionString = "Host=localhost;Port=5432;Database=gracelinecms_test;Username=postgres;Password=postgres";
        public static IConfiguration Configuration { get; private set; }

        static GlobalFixtures()
        {
            var webApplicationFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            { "ConnectionStrings:DefaultConnection", ConnectionString },
                            { "GOOGLE_SMTP_SA_CREDENTIAL", "FakeCredential" }
                        });
                    });
                });
            _dbContextFactory = webApplicationFactory.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            Configuration = webApplicationFactory.Services.GetRequiredService<IConfiguration>();
            HttpClient = webApplicationFactory.CreateClient();
        }

        [OneTimeTearDown]
        public async Task GlobalTeardown()
        {
            using var context = await DbContextFactory.CreateDbContextAsync();
            await context.Database.EnsureDeletedAsync(); // Cleanup after tests
            HttpClient.Dispose();
        }
    }

    public static class ObjectHelpers
    {
        public static DefaultEmailAddressConfig GetDefaultEmailAddressConfig(string? fromAddress = "system@test.com", string? fromName = "System Name")
        {
            return new DefaultEmailAddressConfig()
            {
                FromAddress = fromAddress,
                FromName = fromName
            };
        }
    }

}

