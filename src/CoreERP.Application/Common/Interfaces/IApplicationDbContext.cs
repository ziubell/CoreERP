using CoreERP.Domain.Entities.Anagrafica;
using CoreERP.Domain.Entities.Attivita;
using CoreERP.Domain.Entities.Commerciale;
using CoreERP.Domain.Entities.Comunicazioni;
using CoreERP.Domain.Entities.Rete;
using CoreERP.Domain.Entities.Amministrazione;
using CoreERP.Domain.Entities.Progetti;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext used by handlers.
/// Defined here (Application layer) so handlers don't depend on Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    // Anagrafica
    DbSet<Anagrafica> Anagrafiche { get; }
    DbSet<AnagraficaMeta> AnagraficheMeta { get; }
    DbSet<AnagraficaContatto> AnagraficheContatti { get; }
    DbSet<AnagraficaUser> AnagraficheUsers { get; }
    DbSet<AnagraficaSocial> AnagraficheSocial { get; }

    // Attivita
    DbSet<Attivita> Attivita { get; }
    DbSet<AttivitaUser> AttivitaUsers { get; }
    DbSet<AttivitaToDo> AttivitaToDo { get; }
    DbSet<AttivitaPianificazione> AttivitaPianificazioni { get; }
    DbSet<AttivitaFatturazione> AttivitaFatturazioni { get; }

    // Commerciale
    DbSet<Adesione> Adesioni { get; }
    DbSet<AdesioneArticolo> AdesioniArticoli { get; }
    DbSet<Preventivo> Preventivi { get; }
    DbSet<Contratto> Contratti { get; }
    DbSet<Documento> Documenti { get; }
    DbSet<DocumentoRiga> DocumentiRighe { get; }

    // Comunicazioni
    DbSet<Comunicazione> Comunicazioni { get; }
    DbSet<Messaggio> Messaggi { get; }

    // Rete
    DbSet<Impianto> Impianti { get; }
    DbSet<ApparatoDiRete> ApparatiDiRete { get; }

    // Amministrazione
    DbSet<Movimento> Movimenti { get; }
    DbSet<Scadenziario> Scadenziari { get; }
    DbSet<Bank> Banks { get; }

    // Progetti
    DbSet<Progetto> Progetti { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
