namespace CoreERP.Application.DTOs;

public record ContattoDto(
    int Id,
    string Nome,
    string Cognome,
    string? Email,
    string? Cellulare,
    string? Telefono,
    string? Note,
    DateTime DataCreazione,
    DateTime? DataModifica,
    List<ContattoAnagraficaDto>? Anagrafiche);

public record ContattoAnagraficaDto(
    int AnagraficaId,
    string Denominazione,
    string RuoloContattoNome,
    bool Principale);

public record ContattoListItemDto(
    int Id,
    string Nome,
    string Cognome,
    string? Email,
    string? Cellulare,
    string? Telefono,
    DateTime DataCreazione);

public record CreateContattoRequest
{
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Cellulare { get; init; }
    public string? Telefono { get; init; }
    public string? Note { get; init; }
}

public record UpdateContattoRequest
{
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Cellulare { get; init; }
    public string? Telefono { get; init; }
    public string? Note { get; init; }
}
