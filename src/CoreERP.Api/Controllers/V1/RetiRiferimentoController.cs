using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/reti-riferimento")]
[Authorize]
public class RetiRiferimentoController : ControllerBase
{
    private readonly IReteRiferimentoRepository _repository;

    public RetiRiferimentoController(IReteRiferimentoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? attivo = null)
    {
        var result = await _repository.GetAllAsync(attivo);
        var dtos = result.Select(r => new ReteRiferimentoDto(
            r.Id, r.Nome, r.Descrizione, r.Attivo, r.Ordine));
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReteRiferimento entity)
    {
        var result = await _repository.AddAsync(entity);
        var dto = new ReteRiferimentoDto(
            result.Id, result.Nome, result.Descrizione, result.Attivo, result.Ordine);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReteRiferimento entity)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Nome = entity.Nome;
        existing.Descrizione = entity.Descrizione;
        existing.Attivo = entity.Attivo;
        existing.Ordine = entity.Ordine;
        await _repository.UpdateAsync(existing);

        var dto = new ReteRiferimentoDto(
            existing.Id, existing.Nome, existing.Descrizione, existing.Attivo, existing.Ordine);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _repository.IsInUseAsync(id))
            return Conflict(new { message = "La rete di riferimento è in uso e non può essere eliminata" });

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
