using CoreERP.Domain.Common;
using CoreERP.Domain.Entities.Notifications;
using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationIdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TipoNotifica> TipiNotifica => Set<TipoNotifica>();
    public DbSet<Notifica> Notifiche => Set<Notifica>();
    public DbSet<PreferenzaNotificaUtente> PreferenzeNotificaUtente => Set<PreferenzaNotificaUtente>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TipoNotifica>(entity =>
        {
            entity.ToTable("TipiNotifica");
            entity.HasIndex(e => e.Codice).IsUnique();
            entity.Property(e => e.Codice).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Modulo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descrizione).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Icona).HasMaxLength(50);
            entity.Property(e => e.Colore).HasMaxLength(30);
        });

        builder.Entity<Notifica>(entity =>
        {
            entity.ToTable("Notifiche");
            entity.HasIndex(e => new { e.UserId, e.Letta });
            entity.HasIndex(e => e.DataCreazione);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.MittenteUserId).HasMaxLength(450);
            entity.Property(e => e.Titolo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Messaggio).HasMaxLength(1000);
            entity.Property(e => e.Link).HasMaxLength(500);
            entity.HasOne(e => e.TipoNotifica).WithMany().HasForeignKey(e => e.TipoNotificaId);
        });

        builder.Entity<PreferenzaNotificaUtente>(entity =>
        {
            entity.ToTable("PreferenzeNotificaUtente");
            entity.HasIndex(e => new { e.UserId, e.TipoNotificaId }).IsUnique();
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.HasOne(e => e.TipoNotifica).WithMany().HasForeignKey(e => e.TipoNotificaId);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DataCreazione = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.DataModifica = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
