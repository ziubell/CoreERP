using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreERP.Infrastructure.Persistence.Configurations;

public class AnagraficaConfiguration : IEntityTypeConfiguration<Anagrafica>
{
    public void Configure(EntityTypeBuilder<Anagrafica> builder)
    {
        builder.ToTable("ERP_Anagrafica");

        builder.HasIndex(a => a.CodiceFiscale);
        builder.HasIndex(a => a.PartitaIva);
        builder.HasIndex(a => new { a.Tipo, a.Stato });

        builder.Property(a => a.Nome).HasMaxLength(100);
        builder.Property(a => a.Cognome).HasMaxLength(100);
        builder.Property(a => a.RagioneSociale).HasMaxLength(200);
        builder.Property(a => a.CodiceFiscale).HasMaxLength(16);
        builder.Property(a => a.PartitaIva).HasMaxLength(11);
        builder.Property(a => a.CodiceSDI).HasMaxLength(7);
        builder.Property(a => a.PEC).HasMaxLength(200);
        builder.Property(a => a.Cap).HasMaxLength(5);
        builder.Property(a => a.Provincia).HasMaxLength(2);

        builder.Ignore(a => a.NomeCompleto);
        builder.Ignore(a => a.DomainEvents);

        builder.HasOne(a => a.Padre)
            .WithMany()
            .HasForeignKey(a => a.PadreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Meta)
            .WithOne(m => m.Anagrafica)
            .HasForeignKey(m => m.AnagraficaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Contatti)
            .WithOne(c => c.Anagrafica)
            .HasForeignKey(c => c.AnagraficaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Users)
            .WithOne(u => u.Anagrafica)
            .HasForeignKey(u => u.AnagraficaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Social)
            .WithOne(s => s.Anagrafica)
            .HasForeignKey(s => s.AnagraficaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
