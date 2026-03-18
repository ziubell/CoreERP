using CoreERP.Application.Interfaces;
using CoreERP.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;

namespace CoreERP.Infrastructure.Persistence.Repositories;

public class MotivoDisattivazioneRepository : IMotivoDisattivazioneRepository
{
    private readonly ApplicationDbContext _context;

    public MotivoDisattivazioneRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MotivoDisattivazione>> GetAllAsync(bool? attivo = null)
    {
        var query = _context.MotiviDisattivazione.AsQueryable();
        if (attivo.HasValue)
            query = query.Where(m => m.Attivo == attivo.Value);
        return await query.OrderBy(m => m.Ordine).ThenBy(m => m.Nome).ToListAsync();
    }

    public async Task<MotivoDisattivazione?> GetByIdAsync(int id)
        => await _context.MotiviDisattivazione.FindAsync(id);

    public async Task<MotivoDisattivazione> AddAsync(MotivoDisattivazione motivo)
    {
        _context.MotiviDisattivazione.Add(motivo);
        await _context.SaveChangesAsync();
        return motivo;
    }

    public async Task UpdateAsync(MotivoDisattivazione motivo)
    {
        _context.MotiviDisattivazione.Update(motivo);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsInUseAsync(int id)
        => await _context.Anagrafiche.AnyAsync(a => a.MotivoDisattivazioneId == id);

    public async Task DeleteAsync(int id)
    {
        var motivo = await _context.MotiviDisattivazione.FindAsync(id);
        if (motivo != null)
        {
            _context.MotiviDisattivazione.Remove(motivo);
            await _context.SaveChangesAsync();
        }
    }
}
