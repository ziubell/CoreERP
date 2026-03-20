using CoreERP.Application.DTOs;
using CoreERP.Domain.Enums;

namespace CoreERP.Application.Interfaces;

public interface IAnagraficaService
{
    // Anagrafiche
    Task<(List<AnagraficaListItemDto> Items, int TotalCount)> GetAnagraficheAsync(
        TipoAnagrafica? tipo = null, bool? attivo = null, string? ricerca = null,
        int pagina = 1, int dimensionePagina = 20);
    Task<AnagraficaDto?> GetAnagraficaAsync(int id);
    Task<AnagraficaDto> CreateAnagraficaAsync(CreateAnagraficaRequest request, string userId);
    Task<AnagraficaDto> UpdateAnagraficaAsync(int id, UpdateAnagraficaRequest request, string userId);
    Task DeleteAnagraficaAsync(int id, string userId);
    Task<AnagraficaDto> ConvertiAClienteAsync(int id, string userId);
    Task<AnagraficaDto> DisattivaAsync(int id, DisattivaAnagraficaRequest request, string userId);
    Task<AnagraficaDto> RiattivaAsync(int id, string userId);

    // Contatti dell'anagrafica
    Task<AnagraficaContattoDto> AssociaContattoAsync(int anagraficaId, AssociaContattoRequest request, string userId);
    Task RimuoviContattoAsync(int anagraficaId, int contattoId, string userId);
    Task AggiornaRuoloContattoAsync(int anagraficaId, int contattoId, AggiornaRuoloContattoRequest request, string userId);

    // Storico
    Task<List<StoricoModificaDto>> GetStoricoAsync(string entitaTipo, int entitaId, int pagina = 1, int dimensionePagina = 50);
    Task RestoreAsync(int anagraficaId, int storicoModificaId, string userId);

    // Contatti standalone
    Task<(List<ContattoListItemDto> Items, int TotalCount)> GetContattiAsync(
        string? ricerca = null, int pagina = 1, int dimensionePagina = 20, int? excludeAnagraficaId = null);
    Task<ContattoDto?> GetContattoAsync(int id);
    Task<ContattoDto> CreateContattoAsync(CreateContattoRequest request, string userId);
    Task<ContattoDto> UpdateContattoAsync(int id, UpdateContattoRequest request, string userId);
    Task DeleteContattoAsync(int id, string userId);
    Task<bool> VerificaDuplicatoContattoAsync(string? email, string? cellulare, int? excludeId = null);

    // Verifica duplicato anagrafica
    Task<(bool IsDuplicate, int? AnagraficaId, string? Denominazione)> VerificaDuplicatoAnagraficaAsync(
        string? partitaIva, string? codiceFiscale, int? excludeId = null);
}
