using CoreERP.Application.DTOs;

namespace CoreERP.Application.Interfaces;

public interface IEgonService
{
    Task<List<EgonComuneDto>> SearchCityAsync(string query);
    Task<List<EgonStradaDto>> SearchStreetAsync(string egonComune, string query);
    Task<List<EgonCivicoDto>> SearchCivicAsync(string egonStrada, string query);
    Task<EgonNormalizzazioneDto?> NormalizeAsync(string city, string street, string? fraction = null);
    Task<CoperturaResultDto?> VerificaCoperturaAsync(string street, string city, string? fraction = null);
}
