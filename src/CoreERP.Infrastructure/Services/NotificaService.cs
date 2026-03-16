using System.Collections.Concurrent;
using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Notifications;
using CoreERP.Infrastructure.Email;
using CoreERP.Infrastructure.Hubs;
using CoreERP.Infrastructure.Identity;
using CoreERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Services;

public class NotificaService : INotificaService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificaRepository _notificaRepository;
    private readonly IPreferenzaNotificaRepository _preferenzaRepository;
    private readonly IHubContext<NotificaHub> _hubContext;
    private readonly IEmailService _emailService;
    private readonly ITeamsNotificationService _teamsService;
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly ILogger<NotificaService> _logger;

    private static readonly ConcurrentDictionary<string, TipoNotifica> _tipoCache = new();

    public NotificaService(
        ApplicationDbContext context,
        INotificaRepository notificaRepository,
        IPreferenzaNotificaRepository preferenzaRepository,
        IHubContext<NotificaHub> hubContext,
        IEmailService emailService,
        ITeamsNotificationService teamsService,
        UserManager<ApplicationIdentityUser> userManager,
        ILogger<NotificaService> logger)
    {
        _context = context;
        _notificaRepository = notificaRepository;
        _preferenzaRepository = preferenzaRepository;
        _hubContext = hubContext;
        _emailService = emailService;
        _teamsService = teamsService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task InviaAsync(string userId, string codiceTipoNotifica, string titolo,
        string? messaggio = null, string? link = null, string? mittenteUserId = null)
    {
        var tipo = await GetTipoNotificaAsync(codiceTipoNotifica);
        if (tipo is null)
        {
            _logger.LogWarning("Tipo notifica non trovato: {Codice}", codiceTipoNotifica);
            return;
        }

        // 1. Persist notification (always)
        var notifica = new Notifica
        {
            UserId = userId,
            MittenteUserId = mittenteUserId,
            TipoNotificaId = tipo.Id,
            Titolo = titolo,
            Messaggio = messaggio,
            Link = link,
            TipoNotifica = tipo
        };

        await _notificaRepository.AddAsync(notifica);

        // 2. Resolve sender info
        string? mittenteNome = null;
        string? mittenteAvatar = null;
        if (mittenteUserId is not null)
        {
            var mittente = await _userManager.FindByIdAsync(mittenteUserId);
            if (mittente is not null)
            {
                mittenteNome = $"{mittente.Nome} {mittente.Cognome}".Trim();
                mittenteAvatar = mittente.Foto;
            }
        }

        // 3. Check user preferences
        var preferenza = await _preferenzaRepository.GetByUserAndTipoAsync(userId, tipo.Id);
        bool inviaEmail = preferenza?.Email ?? true;
        bool inviaBrowser = preferenza?.Browser ?? true;
        bool inviaTeams = preferenza?.Teams ?? false;

        // 4. Build DTO for SignalR
        var dto = new NotificaDto(
            notifica.Id,
            notifica.Titolo,
            notifica.Messaggio,
            notifica.Link,
            notifica.Letta,
            notifica.DataCreazione,
            notifica.DataLettura,
            mittenteNome,
            mittenteAvatar,
            new TipoNotificaDto(tipo.Id, tipo.Codice, tipo.Modulo, tipo.Descrizione, tipo.Icona, tipo.Colore));

        // 5. Dispatch to channels
        if (inviaBrowser)
        {
            try
            {
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("NuovaNotifica", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore invio notifica browser a utente {UserId}", userId);
            }
        }

        if (inviaEmail)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user?.Email is not null)
                {
                    await _emailService.SendNotificaEmailAsync(user.Email, titolo, messaggio, link, mittenteNome);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore invio notifica email a utente {UserId}", userId);
            }
        }

        if (inviaTeams)
        {
            try
            {
                await _teamsService.InviaAsync(userId, titolo, messaggio, link);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore invio notifica Teams a utente {UserId}", userId);
            }
        }
    }

    public async Task InviaMultiplaAsync(IEnumerable<string> userIds, string codiceTipoNotifica, string titolo,
        string? messaggio = null, string? link = null, string? mittenteUserId = null)
    {
        foreach (var userId in userIds)
        {
            await InviaAsync(userId, codiceTipoNotifica, titolo, messaggio, link, mittenteUserId);
        }
    }

    private async Task<TipoNotifica?> GetTipoNotificaAsync(string codice)
    {
        if (_tipoCache.TryGetValue(codice, out var cached))
            return cached;

        var tipo = await _context.TipiNotifica
            .FirstOrDefaultAsync(t => t.Codice == codice && t.Attivo);

        if (tipo is not null)
            _tipoCache.TryAdd(codice, tipo);

        return tipo;
    }
}
