using CoreERP.Application.DTOs;
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
        var dtos = result.Select(m => new MetodoPagamentoDto(
            m.Id, m.Nome, m.Codice, m.RichiedeIBAN, m.Attivo, m.Ordine));
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MetodoPagamento metodo)
    {
        var result = await _repository.AddAsync(metodo);
        var dto = new MetodoPagamentoDto(
            result.Id, result.Nome, result.Codice, result.RichiedeIBAN, result.Attivo, result.Ordine);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, dto);
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

        var dto = new MetodoPagamentoDto(
            existing.Id, existing.Nome, existing.Codice, existing.RichiedeIBAN, existing.Attivo, existing.Ordine);
        return Ok(dto);
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
