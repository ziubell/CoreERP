using CoreERP.Domain.Enums;

namespace CoreERP.Application.DTOs;

public record AnagraficaDto(
    int Id,
    int? CodiceCliente,
    TipoSoggetto TipoSoggetto,
    string? RagioneSociale,
    string? Nome,
    string? Cognome,
    string Denominazione,
    TipoAnagrafica Tipo,
    bool Attivo,
    int? MotivoDisattivazioneId,
    string? MotivoDisattivazioneNome,
    DateTime? DataConversione,
    string? PartitaIva,
    string? CodiceFiscale,
    string? CodiceSDI,
    string? PEC,
    string? IndirizzoFatturazione,
    string? CAP,
    string? Citta,
    string? Provincia,
    string? Nazione,
    string? Telefono,
    string? SitoWeb,
    string? Note,
    int? MetodoPagamentoId,
    string? MetodoPagamentoNome,
    string? IBAN,
    PeriodicitaPagamento? PeriodicitaPagamento,
    DateTime DataCreazione,
    DateTime? DataModifica,
    List<AnagraficaContattoDto>? Contatti);

public record AnagraficaContattoDto(
    int ContattoId,
    string Nome,
    string Cognome,
    string? Email,
    string? Cellulare,
    string? Telefono,
    int RuoloContattoId,
    string RuoloContattoNome,
    bool Principale);

public record AnagraficaListItemDto(
    int Id,
    int? CodiceCliente,
    string Denominazione,
    TipoSoggetto TipoSoggetto,
    TipoAnagrafica Tipo,
    bool Attivo,
    string? PartitaIva,
    string? Citta,
    string? Provincia,
    string? Telefono,
    DateTime DataCreazione);

public record CreateAnagraficaRequest
{
    public TipoSoggetto TipoSoggetto { get; init; }
    public string? RagioneSociale { get; init; }
    public string? Nome { get; init; }
    public string? Cognome { get; init; }
    public TipoAnagrafica Tipo { get; init; }
    public string? PartitaIva { get; init; }
    public string? CodiceFiscale { get; init; }
    public string? CodiceSDI { get; init; }
    public string? PEC { get; init; }
    public string? IndirizzoFatturazione { get; init; }
    public string? CAP { get; init; }
    public string? Citta { get; init; }
    public string? Provincia { get; init; }
    public string? Nazione { get; init; }
    public string? Telefono { get; init; }
    public string? SitoWeb { get; init; }
    public string? Note { get; init; }
    public int? MetodoPagamentoId { get; init; }
    public string? IBAN { get; init; }
    public PeriodicitaPagamento? PeriodicitaPagamento { get; init; }
    public CreateContattoRequest? PrimoContatto { get; init; }
    public int? PrimoContattoRuoloId { get; init; }
}

public record UpdateAnagraficaRequest
{
    public TipoSoggetto TipoSoggetto { get; init; }
    public string? RagioneSociale { get; init; }
    public string? Nome { get; init; }
    public string? Cognome { get; init; }
    public string? PartitaIva { get; init; }
    public string? CodiceFiscale { get; init; }
    public string? CodiceSDI { get; init; }
    public string? PEC { get; init; }
    public string? IndirizzoFatturazione { get; init; }
    public string? CAP { get; init; }
    public string? Citta { get; init; }
    public string? Provincia { get; init; }
    public string? Nazione { get; init; }
    public string? Telefono { get; init; }
    public string? SitoWeb { get; init; }
    public string? Note { get; init; }
    public int? MetodoPagamentoId { get; init; }
    public string? IBAN { get; init; }
    public PeriodicitaPagamento? PeriodicitaPagamento { get; init; }
}

public record AssociaContattoRequest(int? ContattoId, int RuoloContattoId, bool Principale, CreateContattoRequest? NuovoContatto);

public record AggiornaRuoloContattoRequest(int RuoloContattoId, bool Principale);

public record DisattivaAnagraficaRequest(int MotivoDisattivazioneId);
