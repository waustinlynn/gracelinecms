using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GracelineCMS.Tests
{
    [SetUpFixture]
    public class GlobalFixtures
    {
        private static readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private static void ResetDatabase()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.Migrate();
            }
        }
        public static IDbContextFactory<AppDbContext> DbContextFactory
        {
            get
            {
                ResetDatabase();
                return _dbContextFactory;
            }
        }
        private static readonly HttpClient _httpClient;
        public static HttpClient HttpClient 
        {
            get 
            {
                return _httpClient;
            }
        }

        public async static Task<HttpResponseMessage> PostAsync<T>(string path, T content)
        {
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return await HttpClient.PostAsync(path, httpContent);
        }

        private static IServiceProvider _serviceProvider;
        public static T GetRequiredService<T>()
        {
            #pragma warning disable CS8714
            return _serviceProvider.GetRequiredService<T>() ?? throw new Exception($"Cannot resolve service for type: {typeof(T)}");
            #pragma warning restore CS8714
        }

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
            _serviceProvider = webApplicationFactory.Services;
            _dbContextFactory = webApplicationFactory.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            ResetDatabase();
            Configuration = webApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _httpClient = webApplicationFactory.CreateClient();
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

