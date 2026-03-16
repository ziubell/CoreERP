using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationIdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrato con successo.");
        }
        catch
        {
            // If migrations fail (e.g. no migrations yet), ensure DB is created
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database creato con EnsureCreated.");
        }

        // Seed roles
        string[] roles = new[] { "admin"};
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Ruolo creato: {Role}", role);
            }
        }

        // Seed users
        var testUsers = new[]
        {
            new
            {
                Email = "pietro.bello@spadhausen.com",
                Nome = "Pietro",
                Cognome = "Bello",
                Ruolo = "admin",
                Cellulare = "+39 366 3804995",
                Dipendente = true,
                Reperibile = true,
                CodiceAgente = "AG001",
                Foto = (string?)null,
                Password = "Mitico78"
            }
        };

        foreach (var testUser in testUsers)
        {
            if (await userManager.FindByEmailAsync(testUser.Email) is not null)
                continue;

            var user = new ApplicationIdentityUser
            {
                UserName = testUser.Email,
                Email = testUser.Email,
                EmailConfirmed = true,
                Nome = testUser.Nome,
                Cognome = testUser.Cognome,
                Ruolo = testUser.Ruolo,
                Cellulare = testUser.Cellulare,
                Dipendente = testUser.Dipendente,
                Reperibile = testUser.Reperibile,
                CodiceAgente = testUser.CodiceAgente,
                Foto = testUser.Foto,
                DataLogin = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(user, testUser.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, testUser.Ruolo);
                logger.LogInformation("Utente creato: {Email} ({Ruolo})", testUser.Email, testUser.Ruolo);
            }
            else
            {
                logger.LogError("Errore creazione utente {Email}: {Errors}",
                    testUser.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
