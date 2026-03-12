using CoreERP.Application.Common.Interfaces;
using CoreERP.Domain.Enums;
using CoreERP.Domain.Events;
using MediatR;

namespace CoreERP.Application.Features.Anagrafica.Commands;

public record CreateAnagraficaCommand : IRequest<int>
{
    public string? Nome { get; init; }
    public string? Cognome { get; init; }
    public string? RagioneSociale { get; init; }
    public bool IsPersonaGiuridica { get; init; }
    public TipoAnagrafica Tipo { get; init; }
    public string? CodiceFiscale { get; init; }
    public string? PartitaIva { get; init; }
    public string? CodiceSDI { get; init; }
    public string? PEC { get; init; }
    public string? Indirizzo { get; init; }
    public string? Civico { get; init; }
    public string? Localita { get; init; }
    public string? Cap { get; init; }
    public string? Provincia { get; init; }
    public string? Email { get; init; }
    public string? Cellulare { get; init; }
}

public sealed class CreateAnagraficaCommandHandler : IRequestHandler<CreateAnagraficaCommand, int>
{
    private readonly IApplicationDbContext _db;

    public CreateAnagraficaCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> Handle(CreateAnagraficaCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.Anagrafica.Anagrafica
        {
            Nome = request.Nome,
            Cognome = request.Cognome,
            RagioneSociale = request.RagioneSociale,
            IsPersonaGiuridica = request.IsPersonaGiuridica,
            Tipo = request.Tipo,
            Stato = StatoAnagrafica.Attivo,
            CodiceFiscale = request.CodiceFiscale,
            PartitaIva = request.PartitaIva,
            CodiceSDI = request.CodiceSDI,
            PEC = request.PEC,
            Indirizzo = request.Indirizzo,
            Civico = request.Civico,
            Localita = request.Localita,
            Cap = request.Cap,
            Provincia = request.Provincia,
            DataCreazione = DateTime.UtcNow,
            BrevoNeedsSync = true
        };

        // Store email/cellulare as Meta (EAV pattern from original)
        if (!string.IsNullOrWhiteSpace(request.Email))
            entity.Meta.Add(new Domain.Entities.Anagrafica.AnagraficaMeta { Chiave = "Email", Valore = request.Email });

        if (!string.IsNullOrWhiteSpace(request.Cellulare))
            entity.Meta.Add(new Domain.Entities.Anagrafica.AnagraficaMeta { Chiave = "Cellulare", Valore = request.Cellulare });

        entity.AddDomainEvent(new AnagraficaCreatedEvent(entity.Id));

        _db.Anagrafiche.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
