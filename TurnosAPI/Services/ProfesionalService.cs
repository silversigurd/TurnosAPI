using TurnosAPI.DTOs;
using TurnosAPI.Exceptions;
using TurnosAPI.Models;
using TurnosAPI.Repositories;

namespace TurnosAPI.Services;

public class ProfesionalService : IProfesionalService
{
    private readonly IProfesionalRepository _repository;

    public ProfesionalService(IProfesionalRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProfesionalResponseDto>> GetAllAsync()
    {
        var profesionales = await _repository.GetAllAsync();
        return profesionales.Select(MapToDto);
    }

    public async Task<ProfesionalResponseDto> GetByIdAsync(int id)
    {
        var profesional = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Profesional", id);
        return MapToDto(profesional);
    }

    public async Task<ProfesionalResponseDto> CreateAsync(CreateProfesionalDto dto)
    {
        var profesional = new Profesional
        {
            Nombre       = dto.Nombre,
            Especialidad = dto.Especialidad
        };
        var created = await _repository.CreateAsync(profesional);
        return MapToDto(created);
    }

    public async Task<ProfesionalResponseDto> UpdateAsync(int id, UpdateProfesionalDto dto)
    {
        var profesional = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Profesional", id);

        profesional.Nombre       = dto.Nombre;
        profesional.Especialidad = dto.Especialidad;

        var updated = await _repository.UpdateAsync(profesional);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var profesional = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Profesional", id);
        await _repository.DeleteAsync(profesional);
    }

    private static ProfesionalResponseDto MapToDto(Profesional p) => new()
    {
        Id           = p.Id,
        Nombre       = p.Nombre,
        Especialidad = p.Especialidad
    };
}
