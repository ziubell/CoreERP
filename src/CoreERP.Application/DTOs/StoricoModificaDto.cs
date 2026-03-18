namespace CoreERP.Application.DTOs;

public record StoricoModificaDto(
    int Id,
    string EntitaTipo,
    int EntitaId,
    string Campo,
    string? ValorePrecedente,
    string? ValoreNuovo,
    string? ValorePrecedenteLabel,
    string? ValoreNuovoLabel,
    DateTime DataModifica,
    string ModificatoDa,
    string? ModificatoDaNome,
    string? Note);

public record RestoreRequest(int StoricoModificaId);
