using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/motivi-disattivazione")]
[Authorize]
public class MotiviDisattivazioneController : ControllerBase
{
    private readonly IMotivoDisattivazioneRepository _repository;

    public MotiviDisattivazioneController(IMotivoDisattivazioneRepository repository)
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
    public async Task<IActionResult> Create([FromBody] MotivoDisattivazione motivo)
    {
        var result = await _repository.AddAsync(motivo);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MotivoDisattivazione motivo)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Nome = motivo.Nome;
        existing.Descrizione = motivo.Descrizione;
        existing.Attivo = motivo.Attivo;
        existing.Ordine = motivo.Ordine;
        await _repository.UpdateAsync(existing);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _repository.IsInUseAsync(id))
            return Conflict(new { message = "Il motivo è in uso e non può essere eliminato" });

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
