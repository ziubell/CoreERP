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
    private readonly ITeamsConfigurationService _teamsConfig;
    private readonly ApplicationDbContext _context;

    public NotificheController(
        INotificaRepository notificaRepository,
        IPreferenzaNotificaRepository preferenzaRepository,
        ITeamsConfigurationService teamsConfig,
        ApplicationDbContext context)
    {
        _notificaRepository = notificaRepository;
        _preferenzaRepository = preferenzaRepository;
        _teamsConfig = teamsConfig;
        _context = context;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> GetNotifiche(
        [FromQuery] bool soloNonLette = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int dimensionePagina = 20,
        [FromQuery] string? ricerca = null,
        [FromQuery] string? modulo = null)
    {
        var userId = GetUserId();
        var notifiche = await _notificaRepository.GetByUserAsync(userId, soloNonLette, pagina, dimensionePagina, ricerca, modulo);

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

    [HttpDelete("bulk")]
    public async Task<IActionResult> EliminaBulk([FromBody] BulkDeleteRequest request)
    {
        await _notificaRepository.EliminaMultipleAsync(request.Ids, GetUserId());
        return NoContent();
    }

    [HttpDelete("tutte")]
    public async Task<IActionResult> EliminaTutte()
    {
        await _notificaRepository.EliminaTutteAsync(GetUserId());
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

    [HttpGet("canali")]
    public IActionResult GetCanaliDisponibili()
    {
        return Ok(new
        {
            email = true,
            browser = true,
            teams = _teamsConfig.IsEnabled,
        });
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

    [HttpGet("impostazioni")]
    public async Task<IActionResult> GetImpostazioni()
    {
        var userId = GetUserId();
        var imp = await _context.ImpostazioniNotificaUtente
            .FirstOrDefaultAsync(i => i.UserId == userId);

        return Ok(new { giorniRetention = imp?.GiorniRetention ?? 90 });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedNotifiche()
    {
        var userId = GetUserId();
        var service = HttpContext.RequestServices.GetRequiredService<INotificaService>();

        await service.InviaAsync(userId, "Anagrafica.Creata",
            "Nuovo contatto: Mario Rossi",
            "È stato creato un nuovo contatto nell'anagrafica.",
            "/anagrafica/1");

        await service.InviaAsync(userId, "Anagrafica.Modificata",
            "Contatto aggiornato: Luigi Bianchi",
            "I dati del contatto Luigi Bianchi sono stati modificati.",
            "/anagrafica/2");

        await service.InviaAsync(userId, "Anagrafica.Eliminata",
            "Contatto eliminato: Anna Verdi",
            "Il contatto Anna Verdi è stato rimosso dall'anagrafica.");

        await service.InviaAsync(userId, "Anagrafica.Creata",
            "Nuovo contatto: Gianluca Neri",
            "È stato creato un nuovo contatto: Gianluca Neri S.r.l.",
            "/anagrafica/4");

        await service.InviaAsync(userId, "Anagrafica.Modificata",
            "Indirizzo aggiornato: Mario Rossi",
            "L'indirizzo di Mario Rossi è stato modificato.",
            "/anagrafica/1");

        return Ok(new { message = "5 notifiche di test create." });
    }

    [HttpPut("impostazioni")]
    public async Task<IActionResult> SalvaImpostazioni([FromBody] ImpostazioniRequest request)
    {
        var userId = GetUserId();
        var imp = await _context.ImpostazioniNotificaUtente
            .FirstOrDefaultAsync(i => i.UserId == userId);

        if (imp is null)
        {
            imp = new Domain.Entities.Notifications.ImpostazioniNotificaUtente
            {
                UserId = userId,
                GiorniRetention = request.GiorniRetention
            };
            _context.ImpostazioniNotificaUtente.Add(imp);
        }
        else
        {
            imp.GiorniRetention = request.GiorniRetention;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public record BulkDeleteRequest(List<int> Ids);
public record ImpostazioniRequest(int GiorniRetention);
