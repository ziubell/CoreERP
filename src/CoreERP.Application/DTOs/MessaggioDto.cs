namespace CoreERP.Application.DTOs;

public record MessaggioDto(
    int Id,
    string EntitaTipo,
    int EntitaId,
    string Testo,
    string UserId,
    string UserNome,
    string? UserAvatar,
    DateTime DataCreazione,
    DateTime? DataModifica,
    bool IsOwner,
    List<AllegatoMessaggioDto> Allegati);

public record AllegatoMessaggioDto(
    int Id,
    string NomeFile,
    string ContentType,
    long Dimensione,
    DateTime DataCaricamento);

public record CreateMessaggioRequest
{
    public string EntitaTipo { get; init; } = string.Empty;
    public int EntitaId { get; init; }
    public string Testo { get; init; } = string.Empty;
}

public record UpdateMessaggioRequest
{
    public string Testo { get; init; } = string.Empty;
}
