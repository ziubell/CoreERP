using System.Security.Claims;
using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AnagraficheController : ControllerBase
{
    private readonly IAnagraficaService _service;
    private readonly ISottoscrizioneNotificaRepository _sottoscrizioneRepo;

    public AnagraficheController(IAnagraficaService service, ISottoscrizioneNotificaRepository sottoscrizioneRepo)
    {
        _service = service;
        _sottoscrizioneRepo = sottoscrizioneRepo;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> GetAnagrafiche(
        [FromQuery] TipoAnagrafica? tipo = null,
        [FromQuery] bool? attivo = null,
        [FromQuery] string? ricerca = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int dimensionePagina = 20)
    {
        var (items, totalCount) = await _service.GetAnagraficheAsync(tipo, attivo, ricerca, pagina, dimensionePagina);
        return Ok(new { items, totalCount, pagina, dimensionePagina });
    }

    [HttpGet("verifica-duplicato")]
    public async Task<IActionResult> VerificaDuplicato(
        [FromQuery] string? partitaIva,
        [FromQuery] string? codiceFiscale,
        [FromQuery] int? excludeId)
    {
        var (isDuplicate, anagraficaId, denominazione) = await _service.VerificaDuplicatoAnagraficaAsync(partitaIva, codiceFiscale, excludeId);
        return Ok(new { isDuplicate, anagraficaId, denominazione });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAnagrafica(int id)
    {
        var result = await _service.GetAnagraficaAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnagrafica([FromBody] CreateAnagraficaRequest request)
    {
        try
        {
            var result = await _service.CreateAnagraficaAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetAnagrafica), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAnagrafica(int id, [FromBody] UpdateAnagraficaRequest request)
    {
        try
        {
            var result = await _service.UpdateAnagraficaAsync(id, request, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAnagrafica(int id)
    {
        try
        {
            await _service.DeleteAnagraficaAsync(id, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/converti")]
    public async Task<IActionResult> ConvertiACliente(int id)
    {
        try
        {
            var result = await _service.ConvertiAClienteAsync(id, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/disattiva")]
    public async Task<IActionResult> Disattiva(int id, [FromBody] DisattivaAnagraficaRequest request)
    {
        try
        {
            var result = await _service.DisattivaAsync(id, request, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/riattiva")]
    public async Task<IActionResult> Riattiva(int id)
    {
        try
        {
            var result = await _service.RiattivaAsync(id, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/storico")]
    public async Task<IActionResult> GetStorico(int id, [FromQuery] int pagina = 1, [FromQuery] int dimensionePagina = 50)
    {
        var result = await _service.GetStoricoAsync("Anagrafica", id, pagina, dimensionePagina);
        return Ok(result);
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, [FromBody] RestoreRequest request)
    {
        try
        {
            await _service.RestoreAsync(id, request.StoricoModificaId, GetUserId());
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/contatti")]
    public async Task<IActionResult> AssociaContatto(int id, [FromBody] AssociaContattoRequest request)
    {
        try
        {
            var result = await _service.AssociaContattoAsync(id, request, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/contatti/{contattoId:int}")]
    public async Task<IActionResult> RimuoviContatto(int id, int contattoId)
    {
        try
        {
            await _service.RimuoviContattoAsync(id, contattoId, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id:int}/contatti/{contattoId:int}")]
    public async Task<IActionResult> AggiornaRuoloContatto(int id, int contattoId, [FromBody] AggiornaRuoloContattoRequest request)
    {
        try
        {
            await _service.AggiornaRuoloContattoAsync(id, contattoId, request, GetUserId());
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/follow")]
    public async Task<IActionResult> Follow(int id)
    {
        var userId = GetUserId();
        await _sottoscrizioneRepo.FollowAsync(userId, "Anagrafica", id);
        return Ok();
    }

    [HttpDelete("{id:int}/follow")]
    public async Task<IActionResult> Unfollow(int id)
    {
        var userId = GetUserId();
        await _sottoscrizioneRepo.UnfollowAsync(userId, "Anagrafica", id);
        return NoContent();
    }
}
