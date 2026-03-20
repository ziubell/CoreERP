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
                r.StreetEgon?.ToString() ?? "",
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
                r.CivicEgon?.ToString() ?? "", r.Civic ?? "")).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore ricerca EGON civico: {EgonStrada} {Query}", egonStrada, query);
            return [];
        }
    }

    public async Task<EgonNormalizzazioneDto?> NormalizeAsync(string city, string street, string? fraction = null)
    {
        try
        {
            var url = $"/normalization?city={Uri.EscapeDataString(city)}&street={Uri.EscapeDataString(street)}";
            if (!string.IsNullOrEmpty(fraction) && fraction.Length > 1)
                url += $"&fraction={Uri.EscapeDataString(fraction)}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EgonNormalizationResult>();
            if (result == null) return null;

            return new EgonNormalizzazioneDto(
                result.Address, result.Zip, result.Section, result.City,
                result.ProvinceShort, result.Latitude, result.Longitude, result.EgonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore normalizzazione EGON: {City} {Street}", city, street);
            return null;
        }
    }
}

// Internal model for EGON normalization response
internal class EgonNormalizationResult
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("zip")]
    public string? Zip { get; set; }

    [JsonPropertyName("section")]
    public string? Section { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("province_short")]
    public string? ProvinceShort { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("egon_id")]
    public long? EgonId { get; set; }
}

// Internal model for EGON API response deserialization
internal class EgonSearchResult
{
    [JsonPropertyName("city_egon")]
    public long? CityEgon { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("street_egon")]
    public long? StreetEgon { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("civic_egon")]
    public long? CivicEgon { get; set; }

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
