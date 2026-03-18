using CoreERP.Domain.Common;
using CoreERP.Domain.Enums;

namespace CoreERP.Domain.Entities.Anagrafica;

public class Anagrafica : SoftDeletableEntity
{
    public int? CodiceCliente { get; set; }

    public TipoSoggetto TipoSoggetto { get; set; }
    public string? RagioneSociale { get; set; }
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string Denominazione { get; set; } = string.Empty;

    public TipoAnagrafica Tipo { get; set; }
    public bool Attivo { get; set; } = true;
    public int? MotivoDisattivazioneId { get; set; }
    public DateTime? DataConversione { get; set; }

    // Dati fiscali
    public string? PartitaIva { get; set; }
    public string? CodiceFiscale { get; set; }
    public string? CodiceSDI { get; set; }
    public string? PEC { get; set; }

    // Indirizzo fatturazione
    public string? IndirizzoFatturazione { get; set; }
    public string? CAP { get; set; }
    public string? Citta { get; set; }
    public string? Provincia { get; set; }
    public string? Nazione { get; set; } = "Italia";

    // Contatti generici
    public string? Telefono { get; set; }
    public string? SitoWeb { get; set; }
    public string? Note { get; set; }

    // Pagamento
    public int? MetodoPagamentoId { get; set; }
    public string? IBAN { get; set; }
    public PeriodicitaPagamento? PeriodicitaPagamento { get; set; }

    // Brevo sync
    public string? BrevoCompanyId { get; set; }
    public DateTime? BrevoSyncAt { get; set; }

    // Navigation
    public MotivoDisattivazione? MotivoDisattivazione { get; set; }
    public MetodoPagamento? MetodoPagamento { get; set; }
    public ICollection<AnagraficaContatto> AnagraficaContatti { get; set; } = new List<AnagraficaContatto>();
}
