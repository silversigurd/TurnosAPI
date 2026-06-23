using TurnosAPI.DTOs;

namespace TurnosAPI.Services;

public interface IClienteService
{
    Task<IEnumerable<ClienteResponseDto>> GetAllAsync();
    Task<ClienteResponseDto> GetByIdAsync(int id);
    Task<ClienteResponseDto> CreateAsync(CreateClienteDto dto);
    Task<ClienteResponseDto> UpdateAsync(int id, UpdateClienteDto dto);
    Task DeleteAsync(int id);
}
