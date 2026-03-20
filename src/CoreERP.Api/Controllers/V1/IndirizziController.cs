using System.Security.Claims;
using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StoricoModifica = CoreERP.Domain.Entities.Anagrafica.StoricoModifica;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/indirizzi")]
[Authorize]
public class IndirizziController : ControllerBase
{
    private readonly IIndirizzoRepository _repository;
    private readonly IAnagraficaRepository _anagraficaRepository;
    private readonly IStoricoModificaRepository _storicoRepository;

    public IndirizziController(IIndirizzoRepository repository, IAnagraficaRepository anagraficaRepository, IStoricoModificaRepository storicoRepository)
    {
        _repository = repository;
        _anagraficaRepository = anagraficaRepository;
        _storicoRepository = storicoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tipo = null,
        [FromQuery] string? ricerca = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int dimensionePagina = 20)
    {
        var items = await _repository.GetAllAsync(tipo, ricerca, pagina, dimensionePagina);
        var totalCount = await _repository.GetCountAsync(tipo, ricerca);

        var dtos = items.Select(i => new IndirizzoListItemDto(
            i.Id,
            i.AnagraficaId,
            i.Anagrafica?.Denominazione ?? "",
            i.IsFatturazione,
            i.IsImpianto,
            i.SottoTipo,
            i.Rete,
            $"{i.Strada} {i.Numero}, {i.Citta} ({i.Provincia})",
            i.Citta,
            i.Provincia));

        return Ok(new { items = dtos, totalCount });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var indirizzo = await _repository.GetByIdAsync(id);
        if (indirizzo == null) return NotFound();

        return Ok(MapToDto(indirizzo));
    }

    [HttpGet("anagrafica/{anagraficaId:int}")]
    public async Task<IActionResult> GetByAnagrafica(int anagraficaId)
    {
        var items = await _repository.GetByAnagraficaIdAsync(anagraficaId);
        return Ok(items.Select(MapToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIndirizzoRequest request)
    {
        var anagrafica = await _anagraficaRepository.GetByIdAsync(request.AnagraficaId);
        if (anagrafica == null) return BadRequest(new { message = "Anagrafica non trovata." });

        var indirizzo = new Indirizzo
        {
            AnagraficaId = request.AnagraficaId,
            IsFatturazione = request.IsFatturazione,
            IsImpianto = request.IsImpianto,
            SottoTipo = request.IsImpianto ? request.SottoTipo : null,
            Rete = request.IsImpianto ? request.Rete : null,
            Strada = request.Strada,
            Numero = request.Numero,
            Frazione = request.Frazione,
            Citta = request.Citta,
            Provincia = request.Provincia,
            Regione = request.Regione,
            CAP = request.CAP,
            Latitudine = request.Latitudine,
            Longitudine = request.Longitudine,
            EgonCivico = request.EgonCivico,
            EgonStrada = request.EgonStrada,
            EgonLocalita = request.EgonLocalita,
            CreatoDA = User.FindFirstValue(ClaimTypes.NameIdentifier),
        };

        var result = await _repository.AddAsync(indirizzo);

        // Traccia nello storico
        var label = $"{request.Strada} {request.Numero}, {request.Citta}";
        var uso = string.Join(", ", new[] { request.IsFatturazione ? "Fatturazione" : null, request.IsImpianto ? "Impianto" : null }.Where(x => x != null));
        await _storicoRepository.AddAsync(new StoricoModifica
        {
            EntitaTipo = "Anagrafica",
            EntitaId = request.AnagraficaId,
            Campo = "Indirizzo",
            ValorePrecedente = null,
            ValoreNuovo = label,
            ValoreNuovoLabel = $"{label} ({uso})",
            DataModifica = DateTime.UtcNow,
            ModificatoDa = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            Note = "Aggiunta indirizzo",
        });

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, MapToDto(result));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIndirizzoRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.IsFatturazione = request.IsFatturazione;
        existing.IsImpianto = request.IsImpianto;
        existing.SottoTipo = request.IsImpianto ? request.SottoTipo : null;
        existing.Rete = request.IsImpianto ? request.Rete : null;
        existing.Strada = request.Strada;
        existing.Numero = request.Numero;
        existing.Frazione = request.Frazione;
        existing.Citta = request.Citta;
        existing.Provincia = request.Provincia;
        existing.Regione = request.Regione;
        existing.CAP = request.CAP;
        existing.Latitudine = request.Latitudine;
        existing.Longitudine = request.Longitudine;
        existing.EgonCivico = request.EgonCivico;
        existing.EgonStrada = request.EgonStrada;
        existing.EgonLocalita = request.EgonLocalita;
        existing.ModificatoDa = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _repository.UpdateAsync(existing);
        return Ok(MapToDto(existing));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        var label = $"{existing.Strada} {existing.Numero}, {existing.Citta}";
        var anagraficaId = existing.AnagraficaId;

        await _repository.DeleteAsync(id);

        // Traccia nello storico
        await _storicoRepository.AddAsync(new StoricoModifica
        {
            EntitaTipo = "Anagrafica",
            EntitaId = anagraficaId,
            Campo = "Indirizzo",
            ValorePrecedente = label,
            ValorePrecedenteLabel = label,
            ValoreNuovo = null,
            DataModifica = DateTime.UtcNow,
            ModificatoDa = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            Note = "Rimozione indirizzo",
        });

        return NoContent();
    }

    private static IndirizzoDto MapToDto(Indirizzo i) => new(
        i.Id, i.AnagraficaId, i.IsFatturazione, i.IsImpianto,
        i.SottoTipo, i.Rete,
        i.Strada, i.Numero, i.Frazione, i.Citta, i.Provincia,
        i.Regione, i.CAP, i.Latitudine, i.Longitudine,
        i.EgonCivico, i.EgonStrada, i.EgonLocalita,
        i.Anagrafica?.Denominazione, i.DataCreazione);
}
