namespace CoreERP.Application.DTOs;

public record IndirizzoDto(
    int Id,
    int AnagraficaId,
    bool IsFatturazione,
    bool IsImpianto,
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
    string? AnagraficaDenominazione,
    DateTime DataCreazione);

public record IndirizzoListItemDto(
    int Id,
    int AnagraficaId,
    string AnagraficaDenominazione,
    bool IsFatturazione,
    bool IsImpianto,
    string? SottoTipo,
    string? Rete,
    string IndirizzoCompleto,
    string Citta,
    string Provincia);

public record CreateIndirizzoRequest
{
    public int AnagraficaId { get; init; }
    public bool IsFatturazione { get; init; }
    public bool IsImpianto { get; init; }
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
}

public record UpdateIndirizzoRequest
{
    public bool IsFatturazione { get; init; }
    public bool IsImpianto { get; init; }
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
}

// EGON API DTOs
public record EgonComuneDto(string EgonComune, string Comune);
public record EgonStradaDto(string EgonStrada, string Strada, string Comune, string Provincia, string? CAP, string? Frazione);
public record EgonCivicoDto(string EgonCivico, string Civico);
public record EgonNormalizzazioneDto(
    string? Address, string? Zip, string? Section, string? City,
    string? ProvinceShort, double? Latitude, double? Longitude, long? EgonId);

// Copertura DTOs
public record CoperturaResultDto(
    bool Coperto,
    List<CoperturaLineaDto> Attivabili,
    List<CoperturaLineaDto> Probabili);

public record CoperturaLineaDto(
    string Tipo,
    string Rete,
    int Download,
    int Upload,
    string? Status,
    int? Distanza,
    string? Descrizione);
