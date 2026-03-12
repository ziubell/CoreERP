using CoreERP.Application.Common.Interfaces;
using CoreERP.Application.Common.Models;
using CoreERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Application.Features.Anagrafica.Queries;

public record GetAnagraficheListQuery : IRequest<PagedResult<AnagraficaDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public TipoAnagrafica? Tipo { get; init; }
    public StatoAnagrafica? Stato { get; init; }
}

public sealed class GetAnagraficheListQueryHandler : IRequestHandler<GetAnagraficheListQuery, PagedResult<AnagraficaDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAnagraficheListQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AnagraficaDto>> Handle(GetAnagraficheListQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Anagrafiche
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (request.Tipo.HasValue)
            query = query.Where(a => a.Tipo == request.Tipo.Value);

        if (request.Stato.HasValue)
            query = query.Where(a => a.Stato == request.Stato.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(a =>
                (a.Nome != null && a.Nome.ToLower().Contains(search)) ||
                (a.Cognome != null && a.Cognome.ToLower().Contains(search)) ||
                (a.RagioneSociale != null && a.RagioneSociale.ToLower().Contains(search)) ||
                (a.CodiceFiscale != null && a.CodiceFiscale.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.DataCreazione)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AnagraficaDto
            {
                Id = a.Id,
                NomeCompleto = a.IsPersonaGiuridica
                    ? a.RagioneSociale ?? string.Empty
                    : ((a.Nome ?? "") + " " + (a.Cognome ?? "")).Trim(),
                Nome = a.Nome,
                Cognome = a.Cognome,
                RagioneSociale = a.RagioneSociale,
                IsPersonaGiuridica = a.IsPersonaGiuridica,
                Tipo = a.Tipo,
                Stato = a.Stato,
                CodiceFiscale = a.CodiceFiscale,
                PartitaIva = a.PartitaIva,
                Indirizzo = a.Indirizzo,
                Cap = a.Cap,
                Provincia = a.Provincia,
                Latitudine = a.Latitudine,
                Longitudine = a.Longitudine
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AnagraficaDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
