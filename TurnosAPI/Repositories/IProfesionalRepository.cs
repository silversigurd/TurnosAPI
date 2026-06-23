using TurnosAPI.Models;

namespace TurnosAPI.Repositories;

public interface IProfesionalRepository
{
    Task<IEnumerable<Profesional>> GetAllAsync();
    Task<Profesional?> GetByIdAsync(int id);
    Task<Profesional> CreateAsync(Profesional profesional);
    Task<Profesional> UpdateAsync(Profesional profesional);
    Task DeleteAsync(Profesional profesional);
}
