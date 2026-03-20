using System.Security.Claims;
using CoreERP.Application.DTOs;
using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Messaggi;
using CoreERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/messaggi")]
[Authorize]
public class MessaggiController : ControllerBase
{
    private readonly IMessaggioRepository _repository;
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public MessaggiController(
        IMessaggioRepository repository,
        UserManager<ApplicationIdentityUser> userManager,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _userManager = userManager;
        _environment = environment;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Ottieni messaggi per entità
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByEntita(
        [FromQuery] string entitaTipo,
        [FromQuery] int entitaId,
        [FromQuery] int pagina = 1,
        [FromQuery] int dimensionePagina = 20)
    {
        var userId = GetUserId();
        var messaggi = await _repository.GetByEntitaAsync(entitaTipo, entitaId, pagina, dimensionePagina);
        var totalCount = await _repository.CountByEntitaAsync(entitaTipo, entitaId);

        var dtos = new List<MessaggioDto>();
        foreach (var m in messaggi)
        {
            dtos.Add(await MapToDto(m, userId));
        }

        return Ok(new { items = dtos, totalCount, pagina, dimensionePagina });
    }

    /// <summary>
    /// Ottieni singolo messaggio
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var messaggio = await _repository.GetByIdAsync(id);
        if (messaggio == null) return NotFound();

        var userId = GetUserId();
        return Ok(await MapToDto(messaggio, userId));
    }

    /// <summary>
    /// Crea un nuovo messaggio
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessaggioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Testo))
            return BadRequest(new { message = "Il testo del messaggio è obbligatorio." });

        var userId = GetUserId();
        var messaggio = new Messaggio
        {
            EntitaTipo = request.EntitaTipo,
            EntitaId = request.EntitaId,
            Testo = request.Testo,
            UserId = userId,
        };

        var created = await _repository.AddAsync(messaggio);

        // Ricarica con Include per avere gli allegati
        var loaded = await _repository.GetByIdAsync(created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, await MapToDto(loaded!, userId));
    }

    /// <summary>
    /// Aggiorna il testo di un messaggio (solo proprietario)
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMessaggioRequest request)
    {
        var messaggio = await _repository.GetByIdAsync(id);
        if (messaggio == null) return NotFound();

        var userId = GetUserId();
        if (messaggio.UserId != userId)
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Testo))
            return BadRequest(new { message = "Il testo del messaggio è obbligatorio." });

        messaggio.Testo = request.Testo;
        await _repository.UpdateAsync(messaggio);

        return Ok(await MapToDto(messaggio, userId));
    }

    /// <summary>
    /// Elimina un messaggio (solo proprietario)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var messaggio = await _repository.GetByIdAsync(id);
        if (messaggio == null) return NotFound();

        var userId = GetUserId();
        if (messaggio.UserId != userId)
            return Forbid();

        // Elimina file allegati dal disco
        foreach (var allegato in messaggio.Allegati)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, allegato.Percorso.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        await _repository.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Upload allegato a un messaggio
    /// </summary>
    [HttpPost("{id:int}/allegati")]
    public async Task<IActionResult> UploadAllegato(int id, IFormFile file)
    {
        var messaggio = await _repository.GetByIdAsync(id);
        if (messaggio == null) return NotFound();

        var userId = GetUserId();
        if (messaggio.UserId != userId)
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Nessun file selezionato." });

        // Limite 10MB
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File troppo grande. Massimo 10MB." });

        // Crea directory upload
        var uploadsDir = Path.Combine(_environment.ContentRootPath, "uploads", "messaggi", id.ToString());
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        // Salva file con nome univoco
        var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{file.FileName}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var allegato = new AllegatoMessaggio
        {
            MessaggioId = id,
            NomeFile = file.FileName,
            Percorso = $"/uploads/messaggi/{id}/{fileName}",
            ContentType = file.ContentType,
            Dimensione = file.Length,
            DataCaricamento = DateTime.UtcNow,
        };

        messaggio.Allegati.Add(allegato);
        await _repository.UpdateAsync(messaggio);

        return Ok(new AllegatoMessaggioDto(
            allegato.Id,
            allegato.NomeFile,
            allegato.ContentType,
            allegato.Dimensione,
            allegato.DataCaricamento));
    }

    /// <summary>
    /// Download allegato
    /// </summary>
    [HttpGet("allegati/{allegatoId:int}/download")]
    public async Task<IActionResult> DownloadAllegato(int allegatoId)
    {
        var allegato = await _repository.GetAllegatoByIdAsync(allegatoId);
        if (allegato == null)
            return NotFound();

        var filePath = Path.Combine(_environment.ContentRootPath, allegato.Percorso.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, allegato.ContentType, allegato.NomeFile);
    }

    private async Task<MessaggioDto> MapToDto(Messaggio messaggio, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(messaggio.UserId);
        var userNome = user?.NomeCompleto ?? "Utente sconosciuto";
        var userAvatar = user?.Foto;

        return new MessaggioDto(
            messaggio.Id,
            messaggio.EntitaTipo,
            messaggio.EntitaId,
            messaggio.Testo,
            messaggio.UserId,
            userNome,
            userAvatar,
            messaggio.DataCreazione,
            messaggio.DataModifica,
            messaggio.UserId == currentUserId,
            messaggio.Allegati.Select(a => new AllegatoMessaggioDto(
                a.Id,
                a.NomeFile,
                a.ContentType,
                a.Dimensione,
                a.DataCaricamento)).ToList());
    }
}
