using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Services;

public class EgonService : IEgonService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EgonService> _logger;

    public EgonService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<EgonService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Egon");
        _logger = logger;

        var baseUrl = configuration["Egon:BaseUrl"] ?? "https://api-egon.spadhausen.cloud";
        var username = configuration["Egon:Username"] ?? "CRM";
        var password = configuration["Egon:Password"] ?? "";

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
    }

    public async Task<List<EgonComuneDto>> SearchCityAsync(string query)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/isearch/city?q={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<List<EgonSearchResult>>();
            return results?.Select(r => new EgonComuneDto(
                r.CityEgon?.ToString() ?? "", r.City ?? "")).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore ricerca EGON comune: {Query}", query);
            return [];
        }
    }

    public async Task<List<EgonStradaDto>> SearchStreetAsync(string egonComune, string query)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/isearch/street?egon_id={Uri.EscapeDataString(egonComune)}&q={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<List<EgonSearchResult>>();
            return results?.Select(r => new EgonStradaDto(
                r.StreetEgon ?? "",
                r.Street ?? "",
                r.City ?? "",
                r.Province ?? "",
                r.ZipCode,
                r.Fraction)).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore ricerca EGON strada: {EgonComune} {Query}", egonComune, query);
            return [];
        }
    }

    public async Task<List<EgonCivicoDto>> SearchCivicAsync(string egonStrada, string query)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/isearch/civic?egon_id={Uri.EscapeDataString(egonStrada)}&q={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<List<EgonSearchResult>>();
            return results?.Select(r => new EgonCivicoDto(
                r.CivicEgon ?? "", r.Civic ?? "")).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore ricerca EGON civico: {EgonStrada} {Query}", egonStrada, query);
            return [];
        }
    }
}

// Internal model for EGON API response deserialization
internal class EgonSearchResult
{
    [JsonPropertyName("city_egon")]
    public long? CityEgon { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("street_egon")]
    public string? StreetEgon { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("civic_egon")]
    public string? CivicEgon { get; set; }

    [JsonPropertyName("civic")]
    public string? Civic { get; set; }

    [JsonPropertyName("province")]
    public string? Province { get; set; }

    [JsonPropertyName("province_egon")]
    public long? ProvinceEgon { get; set; }

    [JsonPropertyName("zip_code")]
    public string? ZipCode { get; set; }

    [JsonPropertyName("fraction")]
    public string? Fraction { get; set; }
}
