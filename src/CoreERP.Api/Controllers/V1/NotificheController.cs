using System.Security.Claims;
using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificheController : ControllerBase
{
    private readonly INotificaRepository _notificaRepository;
    private readonly IPreferenzaNotificaRepository _preferenzaRepository;
    private readonly ApplicationDbContext _context;

    public NotificheController(
        INotificaRepository notificaRepository,
        IPreferenzaNotificaRepository preferenzaRepository,
        ApplicationDbContext context)
    {
        _notificaRepository = notificaRepository;
        _preferenzaRepository = preferenzaRepository;
        _context = context;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> GetNotifiche(
        [FromQuery] bool soloNonLette = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int dimensionePagina = 20)
    {
        var userId = GetUserId();
        var notifiche = await _notificaRepository.GetByUserAsync(userId, soloNonLette, pagina, dimensionePagina);

        // Resolve sender info for notifications with MittenteUserId
        var mittenteIds = notifiche
            .Where(n => n.MittenteUserId is not null)
            .Select(n => n.MittenteUserId!)
            .Distinct()
            .ToList();

        var mittenti = mittenteIds.Count > 0
            ? await _context.Users
                .Where(u => mittenteIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Nome, u.Cognome, u.Foto })
                .ToDictionaryAsync(u => u.Id)
            : new();

        var dtos = notifiche.Select(n =>
        {
            string? mittenteNome = null;
            string? mittenteAvatar = null;
            if (n.MittenteUserId is not null && mittenti.TryGetValue(n.MittenteUserId, out var m))
            {
                mittenteNome = $"{m.Nome} {m.Cognome}".Trim();
                mittenteAvatar = m.Foto;
            }

            return new NotificaDto(
                n.Id, n.Titolo, n.Messaggio, n.Link, n.Letta, n.DataCreazione, n.DataLettura,
                mittenteNome, mittenteAvatar,
                new TipoNotificaDto(n.TipoNotifica.Id, n.TipoNotifica.Codice, n.TipoNotifica.Modulo,
                    n.TipoNotifica.Descrizione, n.TipoNotifica.Icona, n.TipoNotifica.Colore));
        });

        return Ok(dtos);
    }

    [HttpGet("non-lette/count")]
    public async Task<IActionResult> GetContaNonLette()
    {
        var count = await _notificaRepository.ContaNonLetteAsync(GetUserId());
        return Ok(new { count });
    }

    [HttpPut("{id}/letta")]
    public async Task<IActionResult> SegnaComeLetta(int id)
    {
        await _notificaRepository.SegnaComeLettaAsync(id, GetUserId());
        return NoContent();
    }

    [HttpPut("lette")]
    public async Task<IActionResult> SegnaTutteComeLette()
    {
        await _notificaRepository.SegnaTutteComeLettaAsync(GetUserId());
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Elimina(int id)
    {
        await _notificaRepository.EliminaAsync(id, GetUserId());
        return NoContent();
    }

    [HttpGet("tipi")]
    public async Task<IActionResult> GetTipiNotifica()
    {
        var tipi = await _context.TipiNotifica
            .Where(t => t.Attivo)
            .OrderBy(t => t.Modulo)
            .ThenBy(t => t.Codice)
            .Select(t => new TipoNotificaDto(t.Id, t.Codice, t.Modulo, t.Descrizione, t.Icona, t.Colore))
            .ToListAsync();

        return Ok(tipi);
    }

    [HttpGet("preferenze")]
    public async Task<IActionResult> GetPreferenze()
    {
        var preferenze = await _preferenzaRepository.GetByUserAsync(GetUserId());
        var dtos = preferenze.Select(p => new PreferenzaNotificaDto(
            p.TipoNotificaId, p.Email, p.Browser, p.Teams));

        return Ok(dtos);
    }

    [HttpPut("preferenze")]
    public async Task<IActionResult> SalvaPreferenze([FromBody] List<PreferenzaNotificaDto> preferenze)
    {
        await _preferenzaRepository.SalvaPreferenzeAsync(GetUserId(), preferenze);
        return NoContent();
    }
}
