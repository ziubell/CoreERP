using CoreERP.Domain.Common;
using CoreERP.Domain.Entities.Anagrafica;
using CoreERP.Domain.Entities.Messaggi;
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
    public DbSet<SottoscrizioneNotifica> SottoscrizioniNotifica => Set<SottoscrizioneNotifica>();
    public DbSet<ImpostazioniNotificaUtente> ImpostazioniNotificaUtente => Set<ImpostazioniNotificaUtente>();

    // Anagrafica
    public DbSet<Anagrafica> Anagrafiche => Set<Anagrafica>();
    public DbSet<Contatto> Contatti => Set<Contatto>();
    public DbSet<AnagraficaContatto> AnagraficaContatti => Set<AnagraficaContatto>();
    public DbSet<RuoloContatto> RuoliContatto => Set<RuoloContatto>();
    public DbSet<MetodoPagamento> MetodiPagamento => Set<MetodoPagamento>();
    public DbSet<MotivoDisattivazione> MotiviDisattivazione => Set<MotivoDisattivazione>();
    public DbSet<StoricoModifica> StoricoModifiche => Set<StoricoModifica>();
    public DbSet<Indirizzo> Indirizzi => Set<Indirizzo>();
    public DbSet<TipoTecnologia> TipiTecnologia => Set<TipoTecnologia>();
    public DbSet<ReteRiferimento> RetiRiferimento => Set<ReteRiferimento>();

    // Messaggi
    public DbSet<Messaggio> Messaggi => Set<Messaggio>();
    public DbSet<AllegatoMessaggio> AllegatiMessaggio => Set<AllegatoMessaggio>();

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

        builder.Entity<SottoscrizioneNotifica>(entity =>
        {
            entity.ToTable("SottoscrizioniNotifica");
            entity.HasIndex(e => new { e.UserId, e.EntitaTipo, e.EntitaId }).IsUnique();
            entity.HasIndex(e => new { e.EntitaTipo, e.EntitaId });
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.EntitaTipo).HasMaxLength(100).IsRequired();
        });

        builder.Entity<ImpostazioniNotificaUtente>(entity =>
        {
            entity.ToTable("ImpostazioniNotificaUtente");
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
        });

        // === Anagrafica ===

        builder.Entity<Anagrafica>(entity =>
        {
            entity.ToTable("Anagrafiche");
            entity.HasIndex(e => e.CodiceCliente).IsUnique().HasFilter("CodiceCliente IS NOT NULL");
            entity.HasIndex(e => e.PartitaIva).IsUnique().HasFilter("PartitaIva IS NOT NULL");
            entity.HasIndex(e => e.CodiceFiscale).IsUnique().HasFilter("CodiceFiscale IS NOT NULL");
            entity.HasIndex(e => e.Denominazione);
            entity.HasIndex(e => new { e.Tipo, e.Attivo });

            entity.Property(e => e.RagioneSociale).HasMaxLength(200);
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.Cognome).HasMaxLength(100);
            entity.Property(e => e.Denominazione).HasMaxLength(300).IsRequired();
            entity.Property(e => e.PartitaIva).HasMaxLength(16);
            entity.Property(e => e.CodiceFiscale).HasMaxLength(16);
            entity.Property(e => e.CodiceSDI).HasMaxLength(7);
            entity.Property(e => e.PEC).HasMaxLength(100);
            entity.Property(e => e.IndirizzoFatturazione).HasMaxLength(300);
            entity.Property(e => e.CAP).HasMaxLength(10);
            entity.Property(e => e.Citta).HasMaxLength(100);
            entity.Property(e => e.Provincia).HasMaxLength(5);
            entity.Property(e => e.Nazione).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(30);
            entity.Property(e => e.SitoWeb).HasMaxLength(200);
            entity.Property(e => e.Note).HasMaxLength(2000);
            entity.Property(e => e.IBAN).HasMaxLength(34);
            entity.Property(e => e.BrevoCompanyId).HasMaxLength(50);

            entity.HasOne(e => e.MotivoDisattivazione).WithMany().HasForeignKey(e => e.MotivoDisattivazioneId);
            entity.HasOne(e => e.MetodoPagamento).WithMany().HasForeignKey(e => e.MetodoPagamentoId);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        builder.Entity<Indirizzo>(entity =>
        {
            entity.ToTable("Indirizzi");
            entity.HasIndex(e => new { e.AnagraficaId, e.IsFatturazione, e.IsImpianto });

            entity.Property(e => e.SottoTipo).HasMaxLength(50);
            entity.Property(e => e.Rete).HasMaxLength(50);
            entity.Property(e => e.Strada).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Numero).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Frazione).HasMaxLength(100);
            entity.Property(e => e.Citta).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Provincia).HasMaxLength(5).IsRequired();
            entity.Property(e => e.Regione).HasMaxLength(100);
            entity.Property(e => e.CAP).HasMaxLength(10);
            entity.Property(e => e.EgonCivico).HasMaxLength(50);
            entity.Property(e => e.EgonStrada).HasMaxLength(50);
            entity.Property(e => e.EgonLocalita).HasMaxLength(50);

            entity.HasOne(e => e.Anagrafica).WithMany(a => a.Indirizzi).HasForeignKey(e => e.AnagraficaId);
        });

        builder.Entity<Contatto>(entity =>
        {
            entity.ToTable("Contatti");
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("Email IS NOT NULL AND IsDeleted = 0");
            entity.HasIndex(e => e.Cellulare).IsUnique().HasFilter("Cellulare IS NOT NULL AND IsDeleted = 0");

            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Cognome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Cellulare).HasMaxLength(30);
            entity.Property(e => e.Telefono).HasMaxLength(30);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.BrevoContactId).HasMaxLength(50);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        builder.Entity<AnagraficaContatto>(entity =>
        {
            entity.ToTable("AnagraficaContatti");
            entity.HasIndex(e => new { e.AnagraficaId, e.ContattoId }).IsUnique();

            entity.HasOne(e => e.Anagrafica).WithMany(a => a.AnagraficaContatti).HasForeignKey(e => e.AnagraficaId);
            entity.HasOne(e => e.Contatto).WithMany(c => c.AnagraficaContatti).HasForeignKey(e => e.ContattoId);
            entity.HasOne(e => e.RuoloContatto).WithMany().HasForeignKey(e => e.RuoloContattoId);
        });

        builder.Entity<RuoloContatto>(entity =>
        {
            entity.ToTable("RuoliContatto");
            entity.HasIndex(e => e.Nome).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descrizione).HasMaxLength(300);
        });

        builder.Entity<MetodoPagamento>(entity =>
        {
            entity.ToTable("MetodiPagamento");
            entity.HasIndex(e => e.Nome).IsUnique();
            entity.HasIndex(e => e.Codice).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Codice).HasMaxLength(10).IsRequired();
        });

        builder.Entity<MotivoDisattivazione>(entity =>
        {
            entity.ToTable("MotiviDisattivazione");
            entity.HasIndex(e => e.Nome).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descrizione).HasMaxLength(300);
        });

        builder.Entity<TipoTecnologia>(entity =>
        {
            entity.ToTable("TipiTecnologia");
            entity.HasIndex(e => e.Nome).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descrizione).HasMaxLength(300);
        });

        builder.Entity<ReteRiferimento>(entity =>
        {
            entity.ToTable("RetiRiferimento");
            entity.HasIndex(e => e.Nome).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descrizione).HasMaxLength(300);
        });

        builder.Entity<StoricoModifica>(entity =>
        {
            entity.ToTable("StoricoModifiche");
            entity.HasIndex(e => new { e.EntitaTipo, e.EntitaId, e.DataModifica }).IsDescending(false, false, true);

            entity.Property(e => e.EntitaTipo).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Campo).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ValorePrecedente).HasMaxLength(2000);
            entity.Property(e => e.ValoreNuovo).HasMaxLength(2000);
            entity.Property(e => e.ValorePrecedenteLabel).HasMaxLength(500);
            entity.Property(e => e.ValoreNuovoLabel).HasMaxLength(500);
            entity.Property(e => e.ModificatoDa).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Note).HasMaxLength(500);
        });

        // === Messaggi ===

        builder.Entity<Messaggio>(entity =>
        {
            entity.ToTable("Messaggi");
            entity.HasIndex(e => new { e.EntitaTipo, e.EntitaId, e.DataCreazione }).IsDescending(false, false, true);

            entity.Property(e => e.EntitaTipo).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Testo).IsRequired();
        });

        builder.Entity<AllegatoMessaggio>(entity =>
        {
            entity.ToTable("AllegatiMessaggio");

            entity.Property(e => e.NomeFile).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Percorso).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Messaggio)
                .WithMany(m => m.Allegati)
                .HasForeignKey(e => e.MessaggioId)
                .OnDelete(DeleteBehavior.Cascade);
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
