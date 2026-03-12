using CoreERP.Application.Common.Interfaces;
using CoreERP.Domain.Common;
using CoreERP.Domain.Entities.Anagrafica;
using CoreERP.Domain.Entities.Amministrazione;
using CoreERP.Domain.Entities.Attivita;
using CoreERP.Domain.Entities.Commerciale;
using CoreERP.Domain.Entities.Comunicazioni;
using CoreERP.Domain.Entities.Progetti;
using CoreERP.Domain.Entities.Rete;
using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationIdentityUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Anagrafica
    public DbSet<Anagrafica> Anagrafiche => Set<Anagrafica>();
    public DbSet<AnagraficaMeta> AnagraficheMeta => Set<AnagraficaMeta>();
    public DbSet<AnagraficaContatto> AnagraficheContatti => Set<AnagraficaContatto>();
    public DbSet<AnagraficaUser> AnagraficheUsers => Set<AnagraficaUser>();
    public DbSet<AnagraficaSocial> AnagraficheSocial => Set<AnagraficaSocial>();

    // Attivita
    public DbSet<Attivita> Attivita => Set<Attivita>();
    public DbSet<AttivitaUser> AttivitaUsers => Set<AttivitaUser>();
    public DbSet<AttivitaToDo> AttivitaToDo => Set<AttivitaToDo>();
    public DbSet<AttivitaPianificazione> AttivitaPianificazioni => Set<AttivitaPianificazione>();
    public DbSet<AttivitaFatturazione> AttivitaFatturazioni => Set<AttivitaFatturazione>();

    // Commerciale
    public DbSet<Adesione> Adesioni => Set<Adesione>();
    public DbSet<AdesioneArticolo> AdesioniArticoli => Set<AdesioneArticolo>();
    public DbSet<Preventivo> Preventivi => Set<Preventivo>();
    public DbSet<Contratto> Contratti => Set<Contratto>();
    public DbSet<Documento> Documenti => Set<Documento>();
    public DbSet<DocumentoRiga> DocumentiRighe => Set<DocumentoRiga>();

    // Comunicazioni
    public DbSet<Comunicazione> Comunicazioni => Set<Comunicazione>();
    public DbSet<Messaggio> Messaggi => Set<Messaggio>();

    // Rete
    public DbSet<Impianto> Impianti => Set<Impianto>();
    public DbSet<ApparatoDiRete> ApparatiDiRete => Set<ApparatoDiRete>();

    // Amministrazione
    public DbSet<Movimento> Movimenti => Set<Movimento>();
    public DbSet<Scadenziario> Scadenziari => Set<Scadenziario>();
    public DbSet<Bank> Banks => Set<Bank>();

    // Progetti
    public DbSet<Progetto> Progetti => Set<Progetto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [builder]);
            }
        }
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder builder) where T : SoftDeletableEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit fields
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DataCreazione = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.DataModifica = DateTime.UtcNow;
                    break;
            }
        }

        // Soft delete
        foreach (var entry in ChangeTracker.Entries<SoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DataCancellazione = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
