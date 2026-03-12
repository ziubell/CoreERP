using CoreERP.Application.Common.Interfaces;
using CoreERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Application.Features.Anagrafica.Queries;

// DTO
public record AnagraficaDto
{
    public int Id { get; init; }
    public string NomeCompleto { get; init; } = string.Empty;
    public string? Nome { get; init; }
    public string? Cognome { get; init; }
    public string? RagioneSociale { get; init; }
    public bool IsPersonaGiuridica { get; init; }
    public TipoAnagrafica Tipo { get; init; }
    public StatoAnagrafica Stato { get; init; }
    public string? CodiceFiscale { get; init; }
    public string? PartitaIva { get; init; }
    public string? Email { get; init; }
    public string? Cellulare { get; init; }
    public string? Indirizzo { get; init; }
    public string? Cap { get; init; }
    public string? Provincia { get; init; }
    public double? Latitudine { get; init; }
    public double? Longitudine { get; init; }
}

// Query
public record GetAnagraficaByIdQuery(int Id) : IRequest<AnagraficaDto?>;

// Handler
public sealed class GetAnagraficaByIdQueryHandler : IRequestHandler<GetAnagraficaByIdQuery, AnagraficaDto?>
{
    private readonly IApplicationDbContext _db;

    public GetAnagraficaByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AnagraficaDto?> Handle(GetAnagraficaByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.Anagrafiche
            .AsNoTracking()
            .Include(a => a.Meta)
            .FirstOrDefaultAsync(a => a.Id == request.Id && !a.IsDeleted, cancellationToken);

        if (entity is null)
            return null;

        var email = entity.Meta.FirstOrDefault(m => m.Chiave == "Email")?.Valore;
        var cellulare = entity.Meta.FirstOrDefault(m => m.Chiave == "Cellulare")?.Valore;

        return new AnagraficaDto
        {
            Id = entity.Id,
            NomeCompleto = entity.NomeCompleto,
            Nome = entity.Nome,
            Cognome = entity.Cognome,
            RagioneSociale = entity.RagioneSociale,
            IsPersonaGiuridica = entity.IsPersonaGiuridica,
            Tipo = entity.Tipo,
            Stato = entity.Stato,
            CodiceFiscale = entity.CodiceFiscale,
            PartitaIva = entity.PartitaIva,
            Email = email,
            Cellulare = cellulare,
            Indirizzo = entity.Indirizzo,
            Cap = entity.Cap,
            Provincia = entity.Provincia,
            Latitudine = entity.Latitudine,
            Longitudine = entity.Longitudine
        };
    }
}
