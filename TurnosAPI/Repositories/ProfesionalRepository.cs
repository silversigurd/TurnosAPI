using Microsoft.EntityFrameworkCore;
using TurnosAPI.Data;
using TurnosAPI.Models;

namespace TurnosAPI.Repositories;

public class ProfesionalRepository : IProfesionalRepository
{
    private readonly TurnosDbContext _context;

    public ProfesionalRepository(TurnosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Profesional>> GetAllAsync()
        => await _context.Profesionales.ToListAsync();

    public async Task<Profesional?> GetByIdAsync(int id)
        => await _context.Profesionales.FindAsync(id);

    public async Task<Profesional> CreateAsync(Profesional profesional)
    {
        _context.Profesionales.Add(profesional);
        await _context.SaveChangesAsync();
        return profesional;
    }

    public async Task<Profesional> UpdateAsync(Profesional profesional)
    {
        _context.Profesionales.Update(profesional);
        await _context.SaveChangesAsync();
        return profesional;
    }

    public async Task DeleteAsync(Profesional profesional)
    {
        _context.Profesionales.Remove(profesional);
        await _context.SaveChangesAsync();
    }
}
