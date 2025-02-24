using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Communication;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection"),
        optionsBuilder =>
        {
            optionsBuilder.MigrationsAssembly("GracelineCMS");
        }
    )
);
builder.Services.AddSingleton<IEmailClient, GmailClient>(sp =>
{
    var encodedCredential = builder.Configuration.GetValue<string>("GOOGLE_SMTP_SA_CREDENTIAL") ?? throw new ArgumentNullException("GOOGLE_SMTP_SA_CREDENTIAL");
    return new GmailClient(encodedCredential);
});
builder.Services.AddSingleton<IAuthenticationCode, AuthenticationCode>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<ReadinessHealthCheck>("readiness");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "readiness"
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

public class ReadinessHealthCheck() : IHealthCheck
{
    private volatile bool _isReady = false;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _isReady = true;
        return _isReady ? Task.FromResult(HealthCheckResult.Healthy()) : Task.FromResult(HealthCheckResult.Unhealthy());
    }

    // This method can be called once the app is fully ready (e.g., after migrations)
    public void SetReady() => _isReady = true;
}