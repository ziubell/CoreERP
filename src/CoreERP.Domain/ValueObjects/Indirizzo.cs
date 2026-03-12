namespace CoreERP.Domain.ValueObjects;

public record Indirizzo
{
    public string Via { get; init; } = string.Empty;
    public string? Civico { get; init; }
    public string? Localita { get; init; }
    public string? Frazione { get; init; }
    public string Cap { get; init; } = string.Empty;
    public string Provincia { get; init; } = string.Empty;
    public string Stato { get; init; } = "Italia";
    public double? Latitudine { get; init; }
    public double? Longitudine { get; init; }
}
