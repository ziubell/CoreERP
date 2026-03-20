namespace CoreERP.Application.DTOs;

public record IndirizzoDto(
    int Id,
    int AnagraficaId,
    string Tipo,
    string? SottoTipo,
    string? Rete,
    string Strada,
    string Numero,
    string? Frazione,
    string Citta,
    string Provincia,
    string? Regione,
    string? CAP,
    double? Latitudine,
    double? Longitudine,
    string? EgonCivico,
    string? EgonStrada,
    string? EgonLocalita,
    bool Principale,
    string? AnagraficaDenominazione,
    DateTime DataCreazione);

public record IndirizzoListItemDto(
    int Id,
    int AnagraficaId,
    string AnagraficaDenominazione,
    string Tipo,
    string? SottoTipo,
    string? Rete,
    string IndirizzoCompleto,
    string Citta,
    string Provincia,
    bool Principale);

public record CreateIndirizzoRequest
{
    public int AnagraficaId { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string? SottoTipo { get; init; }
    public string? Rete { get; init; }
    public string Strada { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string? Frazione { get; init; }
    public string Citta { get; init; } = string.Empty;
    public string Provincia { get; init; } = string.Empty;
    public string? Regione { get; init; }
    public string? CAP { get; init; }
    public double? Latitudine { get; init; }
    public double? Longitudine { get; init; }
    public string? EgonCivico { get; init; }
    public string? EgonStrada { get; init; }
    public string? EgonLocalita { get; init; }
    public bool Principale { get; init; }
}

public record UpdateIndirizzoRequest
{
    public string Tipo { get; init; } = string.Empty;
    public string? SottoTipo { get; init; }
    public string? Rete { get; init; }
    public string Strada { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string? Frazione { get; init; }
    public string Citta { get; init; } = string.Empty;
    public string Provincia { get; init; } = string.Empty;
    public string? Regione { get; init; }
    public string? CAP { get; init; }
    public double? Latitudine { get; init; }
    public double? Longitudine { get; init; }
    public string? EgonCivico { get; init; }
    public string? EgonStrada { get; init; }
    public string? EgonLocalita { get; init; }
    public bool Principale { get; init; }
}

// EGON API DTOs
public record EgonComuneDto(string EgonComune, string Comune);
public record EgonStradaDto(string EgonStrada, string Strada, string Comune, string Provincia, string? CAP, string? Frazione);
public record EgonCivicoDto(string EgonCivico, string Civico);
