using System.Security.Claims;
using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ContattiController : ControllerBase
{
    private readonly IAnagraficaService _service;

    public ContattiController(IAnagraficaService service)
    {
        _service = service;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> GetContatti(
        [FromQuery] string? ricerca = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int dimensionePagina = 20,
        [FromQuery] int? excludeAnagraficaId = null)
    {
        var (items, totalCount) = await _service.GetContattiAsync(ricerca, pagina, dimensionePagina, excludeAnagraficaId);
        return Ok(new { items, totalCount, pagina, dimensionePagina });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetContatto(int id)
    {
        var result = await _service.GetContattoAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContatto([FromBody] CreateContattoRequest request)
    {
        try
        {
            var result = await _service.CreateContattoAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetContatto), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateContatto(int id, [FromBody] UpdateContattoRequest request)
    {
        try
        {
            var result = await _service.UpdateContattoAsync(id, request, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteContatto(int id)
    {
        try
        {
            await _service.DeleteContattoAsync(id, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:int}/storico")]
    public async Task<IActionResult> GetStorico(int id, [FromQuery] int pagina = 1, [FromQuery] int dimensionePagina = 50)
    {
        var result = await _service.GetStoricoAsync("Contatto", id, pagina, dimensionePagina);
        return Ok(result);
    }

    [HttpGet("verifica-duplicato")]
    public async Task<IActionResult> VerificaDuplicato([FromQuery] string? email, [FromQuery] string? cellulare, [FromQuery] int? excludeId)
    {
        var isDuplicate = await _service.VerificaDuplicatoContattoAsync(email, cellulare, excludeId);
        return Ok(new { isDuplicate });
    }
}
