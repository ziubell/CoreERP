using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/ruoli-contatto")]
[Authorize]
public class RuoliContattoController : ControllerBase
{
    private readonly IRuoloContattoRepository _repository;

    public RuoliContattoController(IRuoloContattoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? attivo = null)
    {
        var result = await _repository.GetAllAsync(attivo);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RuoloContatto ruolo)
    {
        var result = await _repository.AddAsync(ruolo);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RuoloContatto ruolo)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Nome = ruolo.Nome;
        existing.Descrizione = ruolo.Descrizione;
        existing.Attivo = ruolo.Attivo;
        existing.Ordine = ruolo.Ordine;
        await _repository.UpdateAsync(existing);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _repository.IsInUseAsync(id))
            return Conflict(new { message = "Il ruolo è in uso e non può essere eliminato" });

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
