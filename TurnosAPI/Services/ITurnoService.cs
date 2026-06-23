using TurnosAPI.DTOs;

namespace TurnosAPI.Services;

public interface ITurnoService
{
    Task<TurnoResponseDto> CrearTurnoAsync(CreateTurnoDto dto);
    Task<IEnumerable<TurnoConProfesionalDto>> GetTurnosPorClienteAsync(int clienteId);
    // No elimina el registro — cambia el estado a Cancelado
    Task<TurnoResponseDto> CancelarTurnoAsync(int id);
    Task<ProximoDisponibleResponseDto> GetProximoTurnoDisponibleAsync(ProximoDisponibleRequestDto dto);
}
