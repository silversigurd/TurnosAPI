using TurnosAPI.DTOs;
using TurnosAPI.Models;

namespace TurnosAPI.Repositories;

public interface ITurnoRepository
{
    Task<Turno?> GetByIdAsync(int id);
    Task<Turno> CreateAsync(Turno turno);
    Task<Turno> UpdateAsync(Turno turno);

    // Los tres métodos siguientes llaman stored procedures via ADO.NET
    Task<bool> HaySuperposicionAsync(int profesionalId, DateTime inicio, DateTime fin, int? excluirTurnoId = null);
    Task<IEnumerable<TurnoConProfesionalDto>> GetTurnosPorClienteAsync(int clienteId);
    Task<ProximoDisponibleResponseDto?> GetProximoTurnoDisponibleAsync(int profesionalId, int duracionMinutos, DateTime fechaDesde);
}
