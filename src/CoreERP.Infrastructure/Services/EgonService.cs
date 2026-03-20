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
    private readonly HttpClient _coverageClient;
    private readonly ILogger<EgonService> _logger;

    public EgonService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<EgonService> logger)
    {
        _logger = logger;

        var username = configuration["Egon:Username"] ?? "CRM";
        var password = configuration["Egon:Password"] ?? "";
        var authHeader = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        var baseUrl = configuration["Egon:BaseUrl"] ?? "https://api-egon.spadhausen.cloud";
        _httpClient = httpClientFactory.CreateClient("Egon");
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = authHeader;

        var coverageUrl = configuration["Egon:CoverageUrl"] ?? "https://api-coverage.spadhausen.cloud";
        _coverageClient = httpClientFactory.CreateClient("EgonCoverage");
        _coverageClient.BaseAddress = new Uri(coverageUrl);
        _coverageClient.DefaultRequestHeaders.Authorization = authHeader;
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

    public async Task<CoperturaResultDto?> VerificaCoperturaAsync(string street, string city, string? fraction = null)
    {
        try
        {
            var url = $"/address_lookup?street={Uri.EscapeDataString(street)}&city={Uri.EscapeDataString(city)}&filter=false";
            if (!string.IsNullOrEmpty(fraction) && fraction.Length > 1)
                url += $"&fraction={Uri.EscapeDataString(fraction)}";

            var response = await _coverageClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var coverage = await response.Content.ReadFromJsonAsync<CoverageApiResponse>();
            if (coverage == null) return null;

            var attivabili = coverage.Available?.Select(MapLineaAttivabile).ToList() ?? [];
            var probabili = coverage.Probable?.Select(MapLineaProbabile).ToList() ?? [];
            var coperto = attivabili.Count > 0;

            return new CoperturaResultDto(coperto, attivabili, probabili);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore verifica copertura: {City} {Street}", city, street);
            return null;
        }
    }

    private static CoperturaLineaDto MapLineaAttivabile(CoverageLineItem item)
    {
        var status = item.StatusCode switch
        {
            1 => "In programma",
            _ => "Attivabile",
        };

        string? descrizione = BuildDescrizione(item);

        return new CoperturaLineaDto(
            item.PriorityString ?? "", item.SupplierString ?? "",
            item.SpeedDownload, item.SpeedUpload, status,
            item.PriorityString is "FTTC" or "FWA" ? item.CabinetDistance : null,
            descrizione);
    }

    private static CoperturaLineaDto MapLineaProbabile(CoverageLineItem item)
    {
        string? descrizione = BuildDescrizione(item);

        return new CoperturaLineaDto(
            item.PriorityString ?? "", item.SupplierString ?? "",
            item.SpeedDownload, item.SpeedUpload, "Probabile",
            item.PriorityString is "FTTC" or "FWA" ? item.CabinetDistance : null,
            descrizione);
    }

    private static string? BuildDescrizione(CoverageLineItem item)
    {
        var info = item.AdditionalInfo;
        if (info == null) return null;

        return (item.PriorityString, item.SupplierString) switch
        {
            ("FTTC", "Tim") => $"Distanza cabina Mt. {item.CabinetDistance}, Centrale {info.Pop}, Profilo {info.Profile}",
            ("FTTC", _) => $"Distanza cabina Mt. {item.CabinetDistance}",
            ("FWA", "Spadhausen") => $"Distanza {info.Pop} Mt. {item.CabinetDistance}",
            ("FTTH", "Tim") => $"Centrale {info.Pop}, Profilo {info.Profile}",
            ("FTTH", "OpenFiber") => $"PCN {info.Pop}, Cluster {info.Cluster}, ID Building {info.BuildingId}",
            _ => null,
        };
    }
}

// Internal model for coverage API response
internal class CoverageApiResponse
{
    [JsonPropertyName("address_info")]
    public CoverageAddressInfo? AddressInfo { get; set; }

    [JsonPropertyName("available")]
    public List<CoverageLineItem>? Available { get; set; }

    [JsonPropertyName("probable")]
    public List<CoverageLineItem>? Probable { get; set; }

    [JsonPropertyName("complete")]
    public List<CoverageLineItem>? Complete { get; set; }
}

internal class CoverageAddressInfo
{
    [JsonPropertyName("egon_id")]
    public long EgonId { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("province_short")]
    public string? ProvinceShort { get; set; }

    [JsonPropertyName("zip")]
    public string? Zip { get; set; }
}

internal class CoverageLineItem
{
    [JsonPropertyName("priority_string")]
    public string? PriorityString { get; set; }

    [JsonPropertyName("supplier_string")]
    public string? SupplierString { get; set; }

    [JsonPropertyName("speed_download")]
    public int SpeedDownload { get; set; }

    [JsonPropertyName("speed_upload")]
    public int SpeedUpload { get; set; }

    [JsonPropertyName("cabinet_distance")]
    public int CabinetDistance { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("status_code")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("match")]
    public int Match { get; set; }

    [JsonPropertyName("additional_info")]
    public CoverageAdditionalInfo? AdditionalInfo { get; set; }
}

internal class CoverageAdditionalInfo
{
    [JsonPropertyName("building_id")]
    public string? BuildingId { get; set; }

    [JsonPropertyName("pop")]
    public string? Pop { get; set; }

    [JsonPropertyName("cluster")]
    public string? Cluster { get; set; }

    [JsonPropertyName("CLLI")]
    public string? Clli { get; set; }

    [JsonPropertyName("tower_distance")]
    public string? TowerDistance { get; set; }

    [JsonPropertyName("tower_name")]
    public string? TowerName { get; set; }

    [JsonPropertyName("profile")]
    public string? Profile { get; set; }
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
