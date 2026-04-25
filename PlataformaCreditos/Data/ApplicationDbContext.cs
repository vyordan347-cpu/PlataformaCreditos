using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlataformaCreditos.Models;

namespace PlataformaCreditos.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<SolicitudCredito> SolicitudesCredito { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("CK_Cliente_IngresosMensuales", "IngresosMensuales > 0");
            });

        builder.Entity<SolicitudCredito>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("CK_SolicitudCredito_MontoSolicitado", "MontoSolicitado > 0");
            });

        builder.Entity<SolicitudCredito>()
            .HasIndex(s => new { s.ClienteId, s.Estado })
            .HasFilter("Estado = 'Pendiente'")
            .IsUnique();

        builder.Entity<SolicitudCredito>()
            .HasOne(s => s.Cliente)
            .WithMany(c => c.Solicitudes)
            .HasForeignKey(s => s.ClienteId);
    }
}