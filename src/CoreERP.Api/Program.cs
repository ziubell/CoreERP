using System.Globalization;
using System.Text;
using CoreERP.Api.Middleware;
using CoreERP.Infrastructure;
using CoreERP.Infrastructure.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Console.WriteLine("[CoreERP] === AVVIO APPLICAZIONE ===");

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"[CoreERP] Builder creato - Ambiente: {builder.Environment.EnvironmentName}");

// Load .env file for local secrets (not committed to git)
var envFile = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envFile))
{
    var envVars = new Dictionary<string, string?>();
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            continue;
        var idx = trimmed.IndexOf('=');
        if (idx > 0)
        {
            // Convert AzureAd__ClientSecret to AzureAd:ClientSecret for .NET config
            var key = trimmed[..idx].Replace("__", ":");
            envVars[key] = trimmed[(idx + 1)..];
        }
    }
    builder.Configuration.AddInMemoryCollection(envVars);
}

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Culture IT
var cultureInfo = new CultureInfo("it-IT") { NumberFormat = { CurrencySymbol = "€" } };
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Infrastructure layer (DB + Identity)
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication - JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "CoreERP-Development-Key-Change-In-Production-!"))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifiche"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoreERP API",
        Version = "v1",
        Description = "API per il sistema ERP CoreERP"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Inserisci il token JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Database seeding
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("[CoreERP] Seed database in corso...");
    try
    {
        await CoreERP.Infrastructure.Persistence.DatabaseSeeder.SeedAsync(app.Services);
        Console.WriteLine("[CoreERP] Seed database completato.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CoreERP] ERRORE seed database: {ex.Message}");
        app.Logger.LogError(ex, "Errore durante il seed del database");
    }
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreERP API v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

// Serve uploaded files (profile photos, etc.)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificaHub>("/hubs/notifiche");
app.MapHealthChecks("/health");

Console.WriteLine("[CoreERP] Server pronto, avvio in corso...");
app.Run();
