namespace CoreERP.Application.DTOs;

public record MetodoPagamentoDto(
    int Id,
    string Nome,
    string Codice,
    bool RichiedeIBAN,
    bool Attivo,
    int Ordine);

public record MotivoDisattivazioneDto(
    int Id,
    string Nome,
    string? Descrizione,
    bool Attivo,
    int Ordine);

public record RuoloContattoDto(
    int Id,
    string Nome,
    string? Descrizione,
    bool Attivo,
    int Ordine);

public record TipoTecnologiaDto(int Id, string Nome, string? Descrizione, bool Attivo, int Ordine);

public record ReteRiferimentoDto(int Id, string Nome, string? Descrizione, bool Attivo, int Ordine);
