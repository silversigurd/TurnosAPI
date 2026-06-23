using TurnosAPI.DTOs;

namespace TurnosAPI.Services;

public interface IProfesionalService
{
    Task<IEnumerable<ProfesionalResponseDto>> GetAllAsync();
    Task<ProfesionalResponseDto> GetByIdAsync(int id);
    Task<ProfesionalResponseDto> CreateAsync(CreateProfesionalDto dto);
    Task<ProfesionalResponseDto> UpdateAsync(int id, UpdateProfesionalDto dto);
    Task DeleteAsync(int id);
}
