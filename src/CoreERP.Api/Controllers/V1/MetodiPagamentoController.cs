using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/metodi-pagamento")]
[Authorize]
public class MetodiPagamentoController : ControllerBase
{
    private readonly IMetodoPagamentoRepository _repository;

    public MetodiPagamentoController(IMetodoPagamentoRepository repository)
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
    public async Task<IActionResult> Create([FromBody] MetodoPagamento metodo)
    {
        var result = await _repository.AddAsync(metodo);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MetodoPagamento metodo)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Nome = metodo.Nome;
        existing.Codice = metodo.Codice;
        existing.RichiedeIBAN = metodo.RichiedeIBAN;
        existing.Attivo = metodo.Attivo;
        existing.Ordine = metodo.Ordine;
        await _repository.UpdateAsync(existing);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _repository.IsInUseAsync(id))
            return Conflict(new { message = "Il metodo di pagamento è in uso e non può essere eliminato" });

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
