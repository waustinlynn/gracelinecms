using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Communication;
using GracelineCMS.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

//config
builder.Configuration.AddEnvironmentVariables();

//auth

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secret = builder.Configuration.GetValue<string>("AuthenticationSigningSecret") ?? throw new ArgumentNullException("Missing AuthenticationSigningSecret config");
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(secret))
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GlobalAdmin", policy =>
    {
        policy.RequireRole("GlobalAdmin");
    });
});
builder.Services.AddSingleton<IClaimsProvider, ClaimsProvider>(options =>
{
    var globalAdminEmail = builder.Configuration.GetValue<string>("GlobalAdminEmail") ?? throw new ArgumentNullException("Missing GlobalAdminEmail in config");
    return new ClaimsProvider(globalAdminEmail);
});
builder.Services.AddSingleton<ITokenHandler>(options =>
{
    var secret = options.GetRequiredService<IConfiguration>()["AuthenticationSigningSecret"] ?? throw new ArgumentNullException("Missing AuthenticationSigningSecret config");
    return new AppTokenHandler(options.GetRequiredService<IClaimsProvider>(), secret);
});

//app services
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


//core
builder.Services.AddControllers();
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

app.UseAuthentication();
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