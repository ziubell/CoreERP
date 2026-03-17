namespace CoreERP.Application.DTOs;

public record NotificaDto(
    int Id,
    string Titolo,
    string? Messaggio,
    string? Link,
    bool Letta,
    DateTime DataCreazione,
    DateTime? DataLettura,
    string? MittenteNome,
    string? MittenteAvatar,
    TipoNotificaDto TipoNotifica);

public record TipoNotificaDto(
    int Id,
    string Codice,
    string Modulo,
    string Descrizione,
    string? Icona,
    string? Colore);

public record PreferenzaNotificaDto(
    int TipoNotificaId,
    bool Email,
    bool Browser,
    bool Teams);
