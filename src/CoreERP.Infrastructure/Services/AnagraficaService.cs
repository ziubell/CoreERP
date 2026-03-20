using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Application.Validators;
using CoreERP.Domain.Entities.Anagrafica;
using CoreERP.Domain.Enums;
using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CoreERP.Infrastructure.Services;

public class AnagraficaService : IAnagraficaService
{
    private readonly IAnagraficaRepository _anagraficaRepo;
    private readonly IContattoRepository _contattoRepo;
    private readonly IRuoloContattoRepository _ruoloContattoRepo;
    private readonly IMetodoPagamentoRepository _metodoPagamentoRepo;
    private readonly IStoricoModificaRepository _storicoRepo;
    private readonly INotificaService _notificaService;
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly ILogger<AnagraficaService> _logger;

    // Campi esclusi dall'audit
    private static readonly HashSet<string> CampiEsclusiAudit = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Anagrafica.BrevoSyncAt), nameof(Anagrafica.BrevoCompanyId),
        nameof(Anagrafica.DataCreazione), nameof(Anagrafica.CreatoDA),
        nameof(Anagrafica.DataModifica), nameof(Anagrafica.ModificatoDa),
        nameof(Anagrafica.IsDeleted), nameof(Anagrafica.DataCancellazione), nameof(Anagrafica.CancellatoDa),
        nameof(Anagrafica.Denominazione), nameof(Anagrafica.Id),
        // Navigation properties
        nameof(Anagrafica.MotivoDisattivazione), nameof(Anagrafica.MetodoPagamento), nameof(Anagrafica.AnagraficaContatti)
    };

    private static readonly HashSet<string> CampiEsclusiAuditContatto = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Contatto.BrevoSyncAt), nameof(Contatto.BrevoContactId),
        nameof(Contatto.DataCreazione), nameof(Contatto.CreatoDA),
        nameof(Contatto.DataModifica), nameof(Contatto.ModificatoDa),
        nameof(Contatto.IsDeleted), nameof(Contatto.DataCancellazione), nameof(Contatto.CancellatoDa),
        nameof(Contatto.Id), nameof(Contatto.AnagraficaContatti)
    };

    // Campi FK che richiedono label
    private static readonly HashSet<string> CampiFKAnagrafica = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Anagrafica.MetodoPagamentoId), nameof(Anagrafica.MotivoDisattivazioneId)
    };

    public AnagraficaService(
        IAnagraficaRepository anagraficaRepo,
        IContattoRepository contattoRepo,
        IRuoloContattoRepository ruoloContattoRepo,
        IMetodoPagamentoRepository metodoPagamentoRepo,
        IStoricoModificaRepository storicoRepo,
        INotificaService notificaService,
        UserManager<ApplicationIdentityUser> userManager,
        ILogger<AnagraficaService> logger)
    {
        _anagraficaRepo = anagraficaRepo;
        _contattoRepo = contattoRepo;
        _ruoloContattoRepo = ruoloContattoRepo;
        _metodoPagamentoRepo = metodoPagamentoRepo;
        _storicoRepo = storicoRepo;
        _notificaService = notificaService;
        _userManager = userManager;
        _logger = logger;
    }

    // === Anagrafiche ===

    public async Task<(List<AnagraficaListItemDto> Items, int TotalCount)> GetAnagraficheAsync(
        TipoAnagrafica? tipo = null, bool? attivo = null, string? ricerca = null,
        int pagina = 1, int dimensionePagina = 20)
    {
        var items = await _anagraficaRepo.GetListAsync(tipo, attivo, ricerca, pagina, dimensionePagina);
        var count = await _anagraficaRepo.CountAsync(tipo, attivo, ricerca);

        var dtos = items.Select(a => new AnagraficaListItemDto(
            a.Id, a.CodiceCliente, a.Denominazione, a.TipoSoggetto, a.Tipo, a.Attivo,
            a.PartitaIva, a.Citta, a.Provincia, a.Telefono, a.DataCreazione)).ToList();

        return (dtos, count);
    }

    public async Task<AnagraficaDto?> GetAnagraficaAsync(int id)
    {
        var a = await _anagraficaRepo.GetByIdAsync(id, includeContatti: true);
        if (a == null) return null;
        return MapToDto(a);
    }

    public async Task<AnagraficaDto> CreateAnagraficaAsync(CreateAnagraficaRequest request, string userId)
    {
        ValidateAnagraficaFields(request.TipoSoggetto, request.RagioneSociale, request.Nome, request.Cognome);
        ValidateFiscalCodes(request.PartitaIva, request.CodiceFiscale);
        await ValidateUniqueness(request.PartitaIva, request.CodiceFiscale);
        await ValidateIBAN(request.IBAN, request.MetodoPagamentoId);

        var anagrafica = new Anagrafica
        {
            TipoSoggetto = request.TipoSoggetto,
            RagioneSociale = request.RagioneSociale,
            Nome = request.Nome,
            Cognome = request.Cognome,
            Tipo = request.Tipo,
            Attivo = true,
            PartitaIva = request.PartitaIva?.Trim(),
            CodiceFiscale = request.CodiceFiscale?.Trim().ToUpperInvariant(),
            CodiceSDI = request.CodiceSDI?.Trim(),
            PEC = request.PEC?.Trim(),
            IndirizzoFatturazione = request.IndirizzoFatturazione,
            CAP = request.CAP?.Trim(),
            Citta = request.Citta,
            Provincia = request.Provincia?.Trim().ToUpperInvariant(),
            Nazione = request.Nazione ?? "Italia",
            Telefono = request.Telefono,
            SitoWeb = request.SitoWeb,
            Note = request.Note,
            MetodoPagamentoId = request.MetodoPagamentoId,
            IBAN = request.IBAN?.Trim().ToUpperInvariant(),
            PeriodicitaPagamento = request.PeriodicitaPagamento,
            CreatoDA = userId
        };

        // Se creato direttamente come Cliente, genera CodiceCliente
        if (request.Tipo == TipoAnagrafica.Cliente)
        {
            anagrafica.CodiceCliente = await _anagraficaRepo.GetNextCodiceClienteAsync();
            anagrafica.DataConversione = DateTime.UtcNow;
        }

        CalcolaDenominazione(anagrafica);
        anagrafica = await _anagraficaRepo.AddAsync(anagrafica);

        // Primo contatto opzionale
        if (request.PrimoContatto != null)
        {
            var contatto = new Contatto
            {
                Nome = request.PrimoContatto.Nome,
                Cognome = request.PrimoContatto.Cognome,
                Email = request.PrimoContatto.Email?.Trim(),
                Cellulare = request.PrimoContatto.Cellulare?.Trim(),
                Telefono = request.PrimoContatto.Telefono,
                Note = request.PrimoContatto.Note,
                CreatoDA = userId
            };
            contatto = await _contattoRepo.AddAsync(contatto);

            var ac = new AnagraficaContatto
            {
                AnagraficaId = anagrafica.Id,
                ContattoId = contatto.Id,
                RuoloContattoId = request.PrimoContattoRuoloId ?? 1, // default primo ruolo
                Principale = true,
                CreatoDA = userId
            };
            anagrafica.AnagraficaContatti.Add(ac);
            await _anagraficaRepo.UpdateAsync(anagrafica);
        }

        await _notificaService.InviaAFollowersAsync("Anagrafica", anagrafica.Id,
            "Anagrafica.Creata", $"Nuova anagrafica: {anagrafica.Denominazione}",
            mittenteUserId: userId);

        return (await GetAnagraficaAsync(anagrafica.Id))!;
    }

    public async Task<AnagraficaDto> UpdateAnagraficaAsync(int id, UpdateAnagraficaRequest request, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Anagrafica {id} non trovata");

        ValidateAnagraficaFields(request.TipoSoggetto, request.RagioneSociale, request.Nome, request.Cognome);
        ValidateFiscalCodes(request.PartitaIva, request.CodiceFiscale);
        await ValidateUniqueness(request.PartitaIva, request.CodiceFiscale, id);
        await ValidateIBAN(request.IBAN, request.MetodoPagamentoId);

        // Track changes for audit
        var changes = await TrackChangesAsync(anagrafica, request, userId);

        // Apply changes
        anagrafica.TipoSoggetto = request.TipoSoggetto;
        anagrafica.RagioneSociale = request.RagioneSociale;
        anagrafica.Nome = request.Nome;
        anagrafica.Cognome = request.Cognome;
        anagrafica.PartitaIva = request.PartitaIva?.Trim();
        anagrafica.CodiceFiscale = request.CodiceFiscale?.Trim().ToUpperInvariant();
        anagrafica.CodiceSDI = request.CodiceSDI?.Trim();
        anagrafica.PEC = request.PEC?.Trim();
        anagrafica.IndirizzoFatturazione = request.IndirizzoFatturazione;
        anagrafica.CAP = request.CAP?.Trim();
        anagrafica.Citta = request.Citta;
        anagrafica.Provincia = request.Provincia?.Trim().ToUpperInvariant();
        anagrafica.Nazione = request.Nazione;
        anagrafica.Telefono = request.Telefono;
        anagrafica.SitoWeb = request.SitoWeb;
        anagrafica.Note = request.Note;
        anagrafica.MetodoPagamentoId = request.MetodoPagamentoId;
        anagrafica.IBAN = request.IBAN?.Trim().ToUpperInvariant();
        anagrafica.PeriodicitaPagamento = request.PeriodicitaPagamento;
        anagrafica.ModificatoDa = userId;

        CalcolaDenominazione(anagrafica);
        await _anagraficaRepo.UpdateAsync(anagrafica);

        if (changes.Count > 0)
        {
            await _storicoRepo.AddRangeAsync(changes);
            await _notificaService.InviaAFollowersAsync("Anagrafica", id,
                "Anagrafica.Modificata", $"Anagrafica modificata: {anagrafica.Denominazione}",
                mittenteUserId: userId);
        }

        return (await GetAnagraficaAsync(id))!;
    }

    public async Task DeleteAnagraficaAsync(int id, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Anagrafica {id} non trovata");

        anagrafica.IsDeleted = true;
        anagrafica.DataCancellazione = DateTime.UtcNow;
        anagrafica.CancellatoDa = userId;
        await _anagraficaRepo.UpdateAsync(anagrafica);

        await _notificaService.InviaAFollowersAsync("Anagrafica", id,
            "Anagrafica.Eliminata", $"Anagrafica eliminata: {anagrafica.Denominazione}",
            mittenteUserId: userId);
    }

    public async Task<AnagraficaDto> ConvertiAClienteAsync(int id, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Anagrafica {id} non trovata");

        if (anagrafica.Tipo == TipoAnagrafica.Cliente)
            throw new InvalidOperationException("L'anagrafica è già un Cliente");

        var changes = new List<StoricoModifica>
        {
            CreateStoricoEntry("Anagrafica", id, nameof(Anagrafica.Tipo),
                ((int)anagrafica.Tipo).ToString(), ((int)TipoAnagrafica.Cliente).ToString(),
                "Potenziale", "Cliente", userId, "Conversione a Cliente")
        };

        anagrafica.Tipo = TipoAnagrafica.Cliente;
        anagrafica.CodiceCliente = await _anagraficaRepo.GetNextCodiceClienteAsync();
        anagrafica.DataConversione = DateTime.UtcNow;
        anagrafica.ModificatoDa = userId;
        await _anagraficaRepo.UpdateAsync(anagrafica);
        await _storicoRepo.AddRangeAsync(changes);

        await _notificaService.InviaAFollowersAsync("Anagrafica", id,
            "Anagrafica.Modificata", $"Potenziale convertito a Cliente: {anagrafica.Denominazione}",
            mittenteUserId: userId);

        return (await GetAnagraficaAsync(id))!;
    }

    public async Task<AnagraficaDto> DisattivaAsync(int id, DisattivaAnagraficaRequest request, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Anagrafica {id} non trovata");

        if (anagrafica.Tipo == TipoAnagrafica.Potenziale)
            throw new InvalidOperationException("Non è possibile disattivare un Potenziale");

        if (!anagrafica.Attivo)
            throw new InvalidOperationException("L'anagrafica è già disattivata");

        anagrafica.Attivo = false;
        anagrafica.MotivoDisattivazioneId = request.MotivoDisattivazioneId;
        anagrafica.ModificatoDa = userId;
        await _anagraficaRepo.UpdateAsync(anagrafica);

        await _storicoRepo.AddAsync(CreateStoricoEntry("Anagrafica", id, nameof(Anagrafica.Attivo),
            "true", "false", null, null, userId, "Disattivazione"));

        await _notificaService.InviaAFollowersAsync("Anagrafica", id,
            "Anagrafica.Modificata", $"Anagrafica disattivata: {anagrafica.Denominazione}",
            mittenteUserId: userId);

        return (await GetAnagraficaAsync(id))!;
    }

    public async Task<AnagraficaDto> RiattivaAsync(int id, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Anagrafica {id} non trovata");

        if (anagrafica.Attivo)
            throw new InvalidOperationException("L'anagrafica è già attiva");

        anagrafica.Attivo = true;
        anagrafica.MotivoDisattivazioneId = null;
        anagrafica.ModificatoDa = userId;
        await _anagraficaRepo.UpdateAsync(anagrafica);

        await _storicoRepo.AddAsync(CreateStoricoEntry("Anagrafica", id, nameof(Anagrafica.Attivo),
            "false", "true", null, null, userId, "Riattivazione"));

        await _notificaService.InviaAFollowersAsync("Anagrafica", id,
            "Anagrafica.Modificata", $"Anagrafica riattivata: {anagrafica.Denominazione}",
            mittenteUserId: userId);

        return (await GetAnagraficaAsync(id))!;
    }

    // === Contatti dell'anagrafica ===

    public async Task<AnagraficaContattoDto> AssociaContattoAsync(int anagraficaId, AssociaContattoRequest request, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(anagraficaId, includeContatti: true)
            ?? throw new KeyNotFoundException($"Anagrafica {anagraficaId} non trovata");

        Contatto contatto;
        if (request.ContattoId.HasValue)
        {
            contatto = await _contattoRepo.GetByIdAsync(request.ContattoId.Value)
                ?? throw new KeyNotFoundException($"Contatto {request.ContattoId} non trovato");

            if (anagrafica.AnagraficaContatti.Any(ac => ac.ContattoId == request.ContattoId.Value))
                throw new InvalidOperationException("Il contatto è già associato a questa anagrafica");
        }
        else if (request.NuovoContatto != null)
        {
            contatto = new Contatto
            {
                Nome = request.NuovoContatto.Nome,
                Cognome = request.NuovoContatto.Cognome,
                Email = request.NuovoContatto.Email?.Trim(),
                Cellulare = request.NuovoContatto.Cellulare?.Trim(),
                Telefono = request.NuovoContatto.Telefono,
                Note = request.NuovoContatto.Note,
                CreatoDA = userId
            };
            contatto = await _contattoRepo.AddAsync(contatto);
        }
        else
        {
            throw new ArgumentException("Specificare ContattoId o NuovoContatto");
        }

        // Se principale, rimuovi flag dagli altri
        if (request.Principale)
        {
            foreach (var ac in anagrafica.AnagraficaContatti.Where(ac => ac.Principale))
                ac.Principale = false;
        }

        var ruolo = await _ruoloContattoRepo.GetByIdAsync(request.RuoloContattoId)
            ?? throw new KeyNotFoundException($"Ruolo contatto {request.RuoloContattoId} non trovato");

        var associazione = new AnagraficaContatto
        {
            AnagraficaId = anagraficaId,
            ContattoId = contatto.Id,
            RuoloContattoId = request.RuoloContattoId,
            Principale = request.Principale,
            CreatoDA = userId
        };
        anagrafica.AnagraficaContatti.Add(associazione);
        await _anagraficaRepo.UpdateAsync(anagrafica);

        // Traccia nello storico
        await _storicoRepo.AddAsync(CreateStoricoEntry("Anagrafica", anagraficaId, "Contatto",
            null, $"{contatto.Nome} {contatto.Cognome}", null, $"{contatto.Nome} {contatto.Cognome} ({ruolo.Nome})",
            userId, "Associazione contatto"));

        await _notificaService.InviaAFollowersAsync("Anagrafica", anagraficaId,
            "Anagrafica.ContattoAggiunto", $"Contatto {contatto.Nome} {contatto.Cognome} associato a {anagrafica.Denominazione}",
            mittenteUserId: userId);

        return new AnagraficaContattoDto(contatto.Id, contatto.Nome, contatto.Cognome,
            contatto.Email, contatto.Cellulare, contatto.Telefono,
            ruolo.Id, ruolo.Nome, request.Principale);
    }

    public async Task RimuoviContattoAsync(int anagraficaId, int contattoId, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(anagraficaId, includeContatti: true)
            ?? throw new KeyNotFoundException($"Anagrafica {anagraficaId} non trovata");

        var associazione = anagrafica.AnagraficaContatti.FirstOrDefault(ac => ac.ContattoId == contattoId)
            ?? throw new KeyNotFoundException("Associazione non trovata");

        var contatto = await _contattoRepo.GetByIdAsync(contattoId);
        var nomeContatto = contatto != null ? $"{contatto.Nome} {contatto.Cognome}" : contattoId.ToString();

        anagrafica.AnagraficaContatti.Remove(associazione);
        await _anagraficaRepo.UpdateAsync(anagrafica);

        // Traccia nello storico
        await _storicoRepo.AddAsync(CreateStoricoEntry("Anagrafica", anagraficaId, "Contatto",
            nomeContatto, null, nomeContatto, null,
            userId, "Rimozione contatto"));

        await _notificaService.InviaAFollowersAsync("Anagrafica", anagraficaId,
            "Anagrafica.ContattoRimosso", $"Contatto rimosso da {anagrafica.Denominazione}",
            mittenteUserId: userId);
    }

    public async Task AggiornaRuoloContattoAsync(int anagraficaId, int contattoId,
        AggiornaRuoloContattoRequest request, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(anagraficaId, includeContatti: true)
            ?? throw new KeyNotFoundException($"Anagrafica {anagraficaId} non trovata");

        var associazione = anagrafica.AnagraficaContatti.FirstOrDefault(ac => ac.ContattoId == contattoId)
            ?? throw new KeyNotFoundException("Associazione non trovata");

        if (request.Principale)
        {
            foreach (var ac in anagrafica.AnagraficaContatti.Where(ac => ac.Principale && ac.ContattoId != contattoId))
                ac.Principale = false;
        }

        associazione.RuoloContattoId = request.RuoloContattoId;
        associazione.Principale = request.Principale;
        await _anagraficaRepo.UpdateAsync(anagrafica);
    }

    // === Storico ===

    public async Task<List<StoricoModificaDto>> GetStoricoAsync(string entitaTipo, int entitaId,
        int pagina = 1, int dimensionePagina = 50)
    {
        var items = await _storicoRepo.GetByEntitaAsync(entitaTipo, entitaId, pagina, dimensionePagina);

        var userIds = items.Select(s => s.ModificatoDa).Distinct().ToList();
        var usersDict = new Dictionary<string, ApplicationIdentityUser>();
        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                usersDict[userId] = user;
        }

        return items.Select(s =>
        {
            usersDict.TryGetValue(s.ModificatoDa, out var user);
            return new StoricoModificaDto(
                s.Id, s.EntitaTipo, s.EntitaId, s.Campo,
                s.ValorePrecedente, s.ValoreNuovo,
                s.ValorePrecedenteLabel, s.ValoreNuovoLabel,
                s.DataModifica, s.ModificatoDa,
                user?.NomeCompleto,
                user?.Foto,
                s.Note);
        }).ToList();
    }

    public async Task RestoreAsync(int anagraficaId, int storicoModificaId, string userId)
    {
        var anagrafica = await _anagraficaRepo.GetByIdAsync(anagraficaId)
            ?? throw new KeyNotFoundException($"Anagrafica {anagraficaId} non trovata");

        var storico = await _storicoRepo.GetByIdAsync(storicoModificaId)
            ?? throw new KeyNotFoundException($"Storico modifica {storicoModificaId} non trovato");

        if (storico.EntitaTipo != "Anagrafica" || storico.EntitaId != anagraficaId)
            throw new InvalidOperationException("Lo storico non appartiene a questa anagrafica");

        var property = typeof(Anagrafica).GetProperty(storico.Campo);
        if (property == null)
            throw new InvalidOperationException($"Campo {storico.Campo} non trovato");

        // Get current value for new audit entry
        var currentValue = property.GetValue(anagrafica)?.ToString();

        // Set the restored value using reflection
        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        object? restoredValue = storico.ValorePrecedente == null ? null :
            targetType.IsEnum ? Enum.Parse(targetType, storico.ValorePrecedente) :
            Convert.ChangeType(storico.ValorePrecedente, targetType);

        property.SetValue(anagrafica, restoredValue);
        CalcolaDenominazione(anagrafica);
        anagrafica.ModificatoDa = userId;
        await _anagraficaRepo.UpdateAsync(anagrafica);

        // Log the restore as a new audit entry
        await _storicoRepo.AddAsync(CreateStoricoEntry("Anagrafica", anagraficaId, storico.Campo,
            currentValue, storico.ValorePrecedente, null, storico.ValorePrecedenteLabel, userId,
            $"Ripristinato valore precedente (da storico #{storicoModificaId})"));
    }

    // === Contatti standalone ===

    public async Task<(List<ContattoListItemDto> Items, int TotalCount)> GetContattiAsync(
        string? ricerca = null, int pagina = 1, int dimensionePagina = 20, int? excludeAnagraficaId = null)
    {
        var items = await _contattoRepo.GetListAsync(ricerca, pagina, dimensionePagina, excludeAnagraficaId);
        var count = await _contattoRepo.CountAsync(ricerca, excludeAnagraficaId);

        var dtos = items.Select(c => new ContattoListItemDto(
            c.Id, c.Nome, c.Cognome, c.Email, c.Cellulare, c.Telefono, c.DataCreazione)).ToList();

        return (dtos, count);
    }

    public async Task<ContattoDto?> GetContattoAsync(int id)
    {
        var c = await _contattoRepo.GetByIdAsync(id, includeAnagrafiche: true);
        if (c == null) return null;

        var anagrafiche = c.AnagraficaContatti.Select(ac => new ContattoAnagraficaDto(
            ac.AnagraficaId, ac.Anagrafica.Denominazione, ac.RuoloContatto.Nome, ac.Principale)).ToList();

        return new ContattoDto(c.Id, c.Nome, c.Cognome, c.Email, c.Cellulare, c.Telefono,
            c.Note, c.DataCreazione, c.DataModifica, anagrafiche);
    }

    public async Task<ContattoDto> CreateContattoAsync(CreateContattoRequest request, string userId)
    {
        await ValidateContattoUniqueness(request.Email, request.Cellulare);

        var contatto = new Contatto
        {
            Nome = request.Nome,
            Cognome = request.Cognome,
            Email = request.Email?.Trim(),
            Cellulare = request.Cellulare?.Trim(),
            Telefono = request.Telefono,
            Note = request.Note,
            CreatoDA = userId
        };
        contatto = await _contattoRepo.AddAsync(contatto);
        return (await GetContattoAsync(contatto.Id))!;
    }

    public async Task<ContattoDto> UpdateContattoAsync(int id, UpdateContattoRequest request, string userId)
    {
        var contatto = await _contattoRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Contatto {id} non trovato");

        await ValidateContattoUniqueness(request.Email, request.Cellulare, id);

        // Track changes
        var changes = TrackContattoChanges(contatto, request, userId);

        contatto.Nome = request.Nome;
        contatto.Cognome = request.Cognome;
        contatto.Email = request.Email?.Trim();
        contatto.Cellulare = request.Cellulare?.Trim();
        contatto.Telefono = request.Telefono;
        contatto.Note = request.Note;
        contatto.ModificatoDa = userId;
        await _contattoRepo.UpdateAsync(contatto);

        if (changes.Count > 0)
        {
            await _storicoRepo.AddRangeAsync(changes);
            await _notificaService.InviaAFollowersAsync("Contatto", id,
                "Contatto.Modificato", $"Contatto modificato: {contatto.Nome} {contatto.Cognome}",
                mittenteUserId: userId);
        }

        return (await GetContattoAsync(id))!;
    }

    public async Task DeleteContattoAsync(int id, string userId)
    {
        var contatto = await _contattoRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Contatto {id} non trovato");

        contatto.IsDeleted = true;
        contatto.DataCancellazione = DateTime.UtcNow;
        contatto.CancellatoDa = userId;
        await _contattoRepo.UpdateAsync(contatto);
    }

    public async Task<bool> VerificaDuplicatoContattoAsync(string? email, string? cellulare, int? excludeId = null)
    {
        if (!string.IsNullOrWhiteSpace(email) && await _contattoRepo.ExistsEmailAsync(email.Trim(), excludeId))
            return true;
        if (!string.IsNullOrWhiteSpace(cellulare) && await _contattoRepo.ExistsCellulareAsync(cellulare.Trim(), excludeId))
            return true;
        return false;
    }

    // === Private helpers ===

    private static void CalcolaDenominazione(Anagrafica anagrafica)
    {
        anagrafica.Denominazione = anagrafica.TipoSoggetto == TipoSoggetto.PersonaGiuridica
            ? anagrafica.RagioneSociale ?? string.Empty
            : $"{anagrafica.Cognome} {anagrafica.Nome}".Trim();
    }

    private static void ValidateAnagraficaFields(TipoSoggetto tipo, string? ragioneSociale, string? nome, string? cognome)
    {
        if (tipo == TipoSoggetto.PersonaGiuridica && string.IsNullOrWhiteSpace(ragioneSociale))
            throw new ArgumentException("RagioneSociale è obbligatoria per PersonaGiuridica");
        if (tipo == TipoSoggetto.PersonaFisica)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome è obbligatorio per PersonaFisica");
            if (string.IsNullOrWhiteSpace(cognome))
                throw new ArgumentException("Cognome è obbligatorio per PersonaFisica");
        }
    }

    private static void ValidateFiscalCodes(string? partitaIva, string? codiceFiscale)
    {
        if (!FiscalCodeValidator.IsValidPartitaIva(partitaIva))
            throw new ArgumentException("Partita IVA non valida");
        if (!FiscalCodeValidator.IsValidCodiceFiscale(codiceFiscale))
            throw new ArgumentException("Codice Fiscale non valido");
    }

    public async Task<(bool IsDuplicate, int? AnagraficaId, string? Denominazione)> VerificaDuplicatoAnagraficaAsync(
        string? partitaIva, string? codiceFiscale, int? excludeId = null)
    {
        if (!string.IsNullOrWhiteSpace(partitaIva))
        {
            var match = await _anagraficaRepo.GetByPartitaIvaAsync(partitaIva.Trim(), excludeId);
            if (match.HasValue)
                return (true, match.Value.Id, match.Value.Denominazione);
        }

        if (!string.IsNullOrWhiteSpace(codiceFiscale))
        {
            var match = await _anagraficaRepo.GetByCodiceFiscaleAsync(codiceFiscale.Trim(), excludeId);
            if (match.HasValue)
                return (true, match.Value.Id, match.Value.Denominazione);
        }

        return (false, null, null);
    }

    private async Task ValidateUniqueness(string? partitaIva, string? codiceFiscale, int? excludeId = null)
    {
        if (!string.IsNullOrWhiteSpace(partitaIva) && await _anagraficaRepo.ExistsPartitaIvaAsync(partitaIva.Trim(), excludeId))
            throw new InvalidOperationException("Partita IVA già presente nel sistema");
        if (!string.IsNullOrWhiteSpace(codiceFiscale) && await _anagraficaRepo.ExistsCodiceFiscaleAsync(codiceFiscale.Trim(), excludeId))
            throw new InvalidOperationException("Codice Fiscale già presente nel sistema");
    }

    private async Task ValidateIBAN(string? iban, int? metodoPagamentoId)
    {
        if (metodoPagamentoId.HasValue)
        {
            var metodo = await _metodoPagamentoRepo.GetByIdAsync(metodoPagamentoId.Value);
            if (metodo?.RichiedeIBAN == true && string.IsNullOrWhiteSpace(iban))
                throw new ArgumentException($"IBAN obbligatorio per il metodo di pagamento {metodo.Nome}");
        }
    }

    private async Task ValidateContattoUniqueness(string? email, string? cellulare, int? excludeId = null)
    {
        if (!string.IsNullOrWhiteSpace(email) && await _contattoRepo.ExistsEmailAsync(email.Trim(), excludeId))
            throw new InvalidOperationException("Email già presente nel sistema");
        if (!string.IsNullOrWhiteSpace(cellulare) && await _contattoRepo.ExistsCellulareAsync(cellulare.Trim(), excludeId))
            throw new InvalidOperationException("Cellulare già presente nel sistema");
    }

    private async Task<List<StoricoModifica>> TrackChangesAsync(Anagrafica current, UpdateAnagraficaRequest request, string userId)
    {
        var changes = new List<StoricoModifica>();
        var now = DateTime.UtcNow;

        void Track(string campo, string? oldVal, string? newVal, string? oldLabel = null, string? newLabel = null)
        {
            if (oldVal != newVal)
                changes.Add(CreateStoricoEntry("Anagrafica", current.Id, campo, oldVal, newVal, oldLabel, newLabel, userId));
        }

        Track(nameof(Anagrafica.TipoSoggetto), ((int)current.TipoSoggetto).ToString(), ((int)request.TipoSoggetto).ToString(),
            current.TipoSoggetto.ToString(), request.TipoSoggetto.ToString());
        Track(nameof(Anagrafica.RagioneSociale), current.RagioneSociale, request.RagioneSociale);
        Track(nameof(Anagrafica.Nome), current.Nome, request.Nome);
        Track(nameof(Anagrafica.Cognome), current.Cognome, request.Cognome);
        Track(nameof(Anagrafica.PartitaIva), current.PartitaIva, request.PartitaIva?.Trim());
        Track(nameof(Anagrafica.CodiceFiscale), current.CodiceFiscale, request.CodiceFiscale?.Trim().ToUpperInvariant());
        Track(nameof(Anagrafica.CodiceSDI), current.CodiceSDI, request.CodiceSDI?.Trim());
        Track(nameof(Anagrafica.PEC), current.PEC, request.PEC?.Trim());
        Track(nameof(Anagrafica.IndirizzoFatturazione), current.IndirizzoFatturazione, request.IndirizzoFatturazione);
        Track(nameof(Anagrafica.CAP), current.CAP, request.CAP?.Trim());
        Track(nameof(Anagrafica.Citta), current.Citta, request.Citta);
        Track(nameof(Anagrafica.Provincia), current.Provincia, request.Provincia?.Trim().ToUpperInvariant());
        Track(nameof(Anagrafica.Nazione), current.Nazione, request.Nazione);
        Track(nameof(Anagrafica.Telefono), current.Telefono, request.Telefono);
        Track(nameof(Anagrafica.SitoWeb), current.SitoWeb, request.SitoWeb);
        Track(nameof(Anagrafica.Note), current.Note, request.Note);
        Track(nameof(Anagrafica.IBAN), current.IBAN, request.IBAN?.Trim().ToUpperInvariant());
        Track(nameof(Anagrafica.PeriodicitaPagamento), current.PeriodicitaPagamento?.ToString(), request.PeriodicitaPagamento?.ToString());

        // FK con label
        if (current.MetodoPagamentoId != request.MetodoPagamentoId)
        {
            string? oldLabel = current.MetodoPagamento?.Nome;
            string? newLabel = null;
            if (request.MetodoPagamentoId.HasValue)
            {
                var newMetodo = await _metodoPagamentoRepo.GetByIdAsync(request.MetodoPagamentoId.Value);
                newLabel = newMetodo?.Nome;
            }
            Track(nameof(Anagrafica.MetodoPagamentoId),
                current.MetodoPagamentoId?.ToString(), request.MetodoPagamentoId?.ToString(),
                oldLabel, newLabel);
        }

        return changes;
    }

    private List<StoricoModifica> TrackContattoChanges(Contatto current, UpdateContattoRequest request, string userId)
    {
        var changes = new List<StoricoModifica>();

        void Track(string campo, string? oldVal, string? newVal)
        {
            if (oldVal != newVal)
                changes.Add(CreateStoricoEntry("Contatto", current.Id, campo, oldVal, newVal, null, null, userId));
        }

        Track(nameof(Contatto.Nome), current.Nome, request.Nome);
        Track(nameof(Contatto.Cognome), current.Cognome, request.Cognome);
        Track(nameof(Contatto.Email), current.Email, request.Email?.Trim());
        Track(nameof(Contatto.Cellulare), current.Cellulare, request.Cellulare?.Trim());
        Track(nameof(Contatto.Telefono), current.Telefono, request.Telefono);
        Track(nameof(Contatto.Note), current.Note, request.Note);

        return changes;
    }

    private static StoricoModifica CreateStoricoEntry(string entitaTipo, int entitaId, string campo,
        string? oldVal, string? newVal, string? oldLabel, string? newLabel, string userId, string? note = null)
    {
        return new StoricoModifica
        {
            EntitaTipo = entitaTipo,
            EntitaId = entitaId,
            Campo = campo,
            ValorePrecedente = oldVal,
            ValoreNuovo = newVal,
            ValorePrecedenteLabel = oldLabel,
            ValoreNuovoLabel = newLabel,
            DataModifica = DateTime.UtcNow,
            ModificatoDa = userId,
            Note = note
        };
    }

    private AnagraficaDto MapToDto(Anagrafica a)
    {
        var contatti = a.AnagraficaContatti.Select(ac => new AnagraficaContattoDto(
            ac.ContattoId, ac.Contatto.Nome, ac.Contatto.Cognome,
            ac.Contatto.Email, ac.Contatto.Cellulare, ac.Contatto.Telefono,
            ac.RuoloContattoId, ac.RuoloContatto.Nome, ac.Principale)).ToList();

        return new AnagraficaDto(a.Id, a.CodiceCliente, a.TipoSoggetto,
            a.RagioneSociale, a.Nome, a.Cognome, a.Denominazione,
            a.Tipo, a.Attivo, a.MotivoDisattivazioneId,
            a.MotivoDisattivazione?.Nome, a.DataConversione,
            a.PartitaIva, a.CodiceFiscale, a.CodiceSDI, a.PEC,
            a.IndirizzoFatturazione, a.CAP, a.Citta, a.Provincia, a.Nazione,
            a.Telefono, a.SitoWeb, a.Note,
            a.MetodoPagamentoId, a.MetodoPagamento?.Nome,
            a.IBAN, a.PeriodicitaPagamento,
            a.DataCreazione, a.DataModifica, contatti);
    }
}
