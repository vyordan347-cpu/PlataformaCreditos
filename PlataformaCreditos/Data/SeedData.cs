using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlataformaCreditos.Models;

namespace PlataformaCreditos.Data;

public static class SeedData
{
    public static async Task InicializarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Analista"))
        {
            await roleManager.CreateAsync(new IdentityRole("Analista"));
        }

        var analistaEmail = "analista@test.com";
        var analistaPassword = "Password123!";

        var analista = await userManager.FindByEmailAsync(analistaEmail);

        if (analista == null)
        {
            analista = new IdentityUser
            {
                UserName = analistaEmail,
                Email = analistaEmail,
                EmailConfirmed = true
            };

        var resultado = await userManager.CreateAsync(analista, analistaPassword);

    if (!resultado.Succeeded)
    {
            throw new Exception("Error creando usuario analista: " +
            string.Join(" | ", resultado.Errors.Select(e => e.Description)));
    }
    }
    else
        {
            analista.EmailConfirmed = true;
            analista.UserName = analistaEmail;
            analista.Email = analistaEmail;

            await userManager.UpdateAsync(analista);

            var token = await userManager.GeneratePasswordResetTokenAsync(analista);
            await userManager.ResetPasswordAsync(analista, token, analistaPassword);
        }

        if (!await userManager.IsInRoleAsync(analista, "Analista"))
        {
            await userManager.AddToRoleAsync(analista, "Analista");
        }

        if (!context.Clientes.Any())
        {
            var cliente1 = new Cliente
            {
                UsuarioId = analista.Id,
                IngresosMensuales = 2000,
                Activo = true
            };

            var cliente2 = new Cliente
            {
                UsuarioId = analista.Id,
                IngresosMensuales = 3000,
                Activo = true
            };

            context.Clientes.AddRange(cliente1, cliente2);
            await context.SaveChangesAsync();

            context.SolicitudesCredito.AddRange(
                new SolicitudCredito
                {
                    ClienteId = cliente1.Id,
                    MontoSolicitado = 800,
                    FechaSolicitud = DateTime.Now,
                    Estado = "Pendiente"
                },
                new SolicitudCredito
                {
                    ClienteId = cliente2.Id,
                    MontoSolicitado = 1200,
                    FechaSolicitud = DateTime.Now,
                    Estado = "Aprobado"
                }
            );

            await context.SaveChangesAsync();
        }
    }
}