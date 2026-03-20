using System.Security.Claims;
using CoreERP.Application.Interfaces;
using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SottoscrizioniController : ControllerBase
{
    private readonly ISottoscrizioneNotificaRepository _repository;
    private readonly UserManager<ApplicationIdentityUser> _userManager;

    public SottoscrizioniController(ISottoscrizioneNotificaRepository repository, UserManager<ApplicationIdentityUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    [HttpGet("{entitaTipo}/{entitaId}")]
    public async Task<IActionResult> GetSottoscrizione(string entitaTipo, int entitaId)
    {
        var sub = await _repository.GetAsync(GetUserId(), entitaTipo, entitaId);
        if (sub == null)
            return Ok(new { following = false });

        return Ok(new
        {
            following = true,
            notificaModifiche = sub.NotificaModifiche,
            notificaMessaggi = sub.NotificaMessaggi,
            notificaContatti = sub.NotificaContatti,
            notificaIndirizzi = sub.NotificaIndirizzi,
        });
    }

    [HttpPost("{entitaTipo}/{entitaId}")]
    public async Task<IActionResult> Follow(string entitaTipo, int entitaId, [FromBody] FollowRequest? request = null)
    {
        var userId = GetUserId();
        var existing = await _repository.GetAsync(userId, entitaTipo, entitaId);

        if (existing != null)
        {
            // Aggiorna preferenze
            if (request != null)
            {
                existing.NotificaModifiche = request.NotificaModifiche;
                existing.NotificaMessaggi = request.NotificaMessaggi;
                existing.NotificaContatti = request.NotificaContatti;
                existing.NotificaIndirizzi = request.NotificaIndirizzi;
                await _repository.UpdateAsync(existing);
            }
        }
        else
        {
            await _repository.FollowAsync(userId, entitaTipo, entitaId);

            if (request != null)
            {
                var sub = await _repository.GetAsync(userId, entitaTipo, entitaId);
                if (sub != null)
                {
                    sub.NotificaModifiche = request.NotificaModifiche;
                    sub.NotificaMessaggi = request.NotificaMessaggi;
                    sub.NotificaContatti = request.NotificaContatti;
                    sub.NotificaIndirizzi = request.NotificaIndirizzi;
                    await _repository.UpdateAsync(sub);
                }
            }
        }

        return NoContent();
    }

    [HttpDelete("{entitaTipo}/{entitaId}")]
    public async Task<IActionResult> Unfollow(string entitaTipo, int entitaId)
    {
        await _repository.UnfollowAsync(GetUserId(), entitaTipo, entitaId);
        return NoContent();
    }

    [HttpGet("{entitaTipo}/{entitaId}/followers")]
    public async Task<IActionResult> GetFollowers(string entitaTipo, int entitaId)
    {
        var followerIds = await _repository.GetFollowersAsync(entitaTipo, entitaId);
        var followers = new List<object>();

        foreach (var userId in followerIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                followers.Add(new
                {
                    userId,
                    nome = user.NomeCompleto,
                    avatar = user.Foto,
                });
            }
        }

        return Ok(new { followers, count = followers.Count });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMySottoscrizioni([FromQuery] string? ricerca = null, [FromQuery] string? entitaTipo = null)
    {
        var subs = await _repository.GetByUserAsync(GetUserId(), entitaTipo);
        var result = new List<object>();

        // Per ora supportiamo solo Anagrafica — in futuro si estende
        var anagraficaIds = subs.Where(s => s.EntitaTipo == "Anagrafica").Select(s => s.EntitaId).ToList();
        if (anagraficaIds.Count > 0)
        {
            // Carichiamo le anagrafiche per avere denominazione e tipo
            using var scope = HttpContext.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CoreERP.Infrastructure.Persistence.ApplicationDbContext>();

            var anagrafiche = await dbContext.Anagrafiche
                .Where(a => anagraficaIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Denominazione, a.Tipo, a.Attivo })
                .ToListAsync();

            foreach (var sub in subs.Where(s => s.EntitaTipo == "Anagrafica"))
            {
                var ana = anagrafiche.FirstOrDefault(a => a.Id == sub.EntitaId);
                if (ana == null) continue;

                // Filtro ricerca
                if (!string.IsNullOrEmpty(ricerca) && !ana.Denominazione.Contains(ricerca, StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(new
                {
                    id = sub.Id,
                    entitaTipo = sub.EntitaTipo,
                    entitaId = sub.EntitaId,
                    denominazione = ana.Denominazione,
                    tipo = ana.Tipo,
                    attivo = ana.Attivo,
                    dataSottoscrizione = sub.DataSottoscrizione,
                    notificaModifiche = sub.NotificaModifiche,
                    notificaMessaggi = sub.NotificaMessaggi,
                    notificaContatti = sub.NotificaContatti,
                    notificaIndirizzi = sub.NotificaIndirizzi,
                });
            }
        }

        return Ok(result);
    }
}

public record FollowRequest
{
    public bool NotificaModifiche { get; init; } = true;
    public bool NotificaMessaggi { get; init; } = true;
    public bool NotificaContatti { get; init; } = true;
    public bool NotificaIndirizzi { get; init; } = true;
}
