using CoreERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/egon")]
[Authorize]
public class EgonController : ControllerBase
{
    private readonly IEgonService _egonService;

    public EgonController(IEgonService egonService)
    {
        _egonService = egonService;
    }

    [HttpGet("comuni")]
    public async Task<IActionResult> SearchComuni([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 3)
            return Ok(Array.Empty<object>());

        var result = await _egonService.SearchCityAsync(q);
        return Ok(result);
    }

    [HttpGet("strade")]
    public async Task<IActionResult> SearchStrade([FromQuery] string egonComune, [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(egonComune) || string.IsNullOrWhiteSpace(q) || q.Length < 3)
            return Ok(Array.Empty<object>());

        var result = await _egonService.SearchStreetAsync(egonComune, q);
        return Ok(result);
    }

    [HttpGet("civici")]
    public async Task<IActionResult> SearchCivici([FromQuery] string egonStrada, [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(egonStrada) || string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<object>());

        var result = await _egonService.SearchCivicAsync(egonStrada, q);
        return Ok(result);
    }

    [HttpGet("normalizza")]
    public async Task<IActionResult> Normalize([FromQuery] string city, [FromQuery] string street, [FromQuery] string? fraction = null)
    {
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(street))
            return BadRequest(new { message = "City e street sono obbligatori" });

        var result = await _egonService.NormalizeAsync(city, street, fraction);
        if (result == null)
            return NotFound(new { message = "Indirizzo non trovato" });

        return Ok(result);
    }
}
