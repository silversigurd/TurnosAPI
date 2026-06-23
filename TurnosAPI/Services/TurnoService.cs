using TurnosAPI.DTOs;
using TurnosAPI.Exceptions;
using TurnosAPI.Models;
using TurnosAPI.Repositories;

namespace TurnosAPI.Services;

public class TurnoService : ITurnoService
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProfesionalRepository _profesionalRepository;

    public TurnoService(
        ITurnoRepository turnoRepository,
        IClienteRepository clienteRepository,
        IProfesionalRepository profesionalRepository)
    {
        _turnoRepository       = turnoRepository;
        _clienteRepository     = clienteRepository;
        _profesionalRepository = profesionalRepository;
    }

    public async Task<TurnoResponseDto> CrearTurnoAsync(CreateTurnoDto dto)
    {
        if (dto.FechaHoraFin <= dto.FechaHoraInicio)
            throw new BusinessRuleException("La fecha/hora de fin debe ser posterior a la de inicio.");

        if (dto.FechaHoraInicio < DateTime.Now)
            throw new BusinessRuleException("No se puede crear un turno con fecha de inicio en el pasado.");

        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId)
            ?? throw new NotFoundException("Cliente", dto.ClienteId);

        var profesional = await _profesionalRepository.GetByIdAsync(dto.ProfesionalId)
            ?? throw new NotFoundException("Profesional", dto.ProfesionalId);

        // Validar superposición de horarios vía stored procedure
        var haySuperposicion = await _turnoRepository.HaySuperposicionAsync(
            dto.ProfesionalId, dto.FechaHoraInicio, dto.FechaHoraFin);

        if (haySuperposicion)
            throw new ConflictException(
                $"El/la profesional {profesional.Nombre} ya tiene un turno que se superpone " +
                $"con el horario solicitado " +
                $"({dto.FechaHoraInicio:dd/MM/yyyy HH:mm} - {dto.FechaHoraFin:HH:mm}). " +
                $"Consultá el endpoint /proximo-disponible para encontrar un horario libre.");

        var turno = new Turno
        {
            ClienteId       = dto.ClienteId,
            ProfesionalId   = dto.ProfesionalId,
            FechaHoraInicio = dto.FechaHoraInicio,
            FechaHoraFin    = dto.FechaHoraFin,
            Estado          = EstadoTurno.Pendiente
        };

        var created = await _turnoRepository.CreateAsync(turno);
        return MapToDto(created);
    }

    public async Task<IEnumerable<TurnoConProfesionalDto>> GetTurnosPorClienteAsync(int clienteId)
    {
        // Verificar que el cliente exista antes de buscar sus turnos
        _ = await _clienteRepository.GetByIdAsync(clienteId)
            ?? throw new NotFoundException("Cliente", clienteId);

        return await _turnoRepository.GetTurnosPorClienteAsync(clienteId);
    }

    // Soft delete: cambia el estado a Cancelado sin borrar el registro
    public async Task<TurnoResponseDto> CancelarTurnoAsync(int id)
    {
        var turno = await _turnoRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Turno", id);

        if (turno.Estado == EstadoTurno.Cancelado)
            throw new BusinessRuleException("El turno ya se encuentra cancelado.");

        turno.Estado = EstadoTurno.Cancelado;
        var updated = await _turnoRepository.UpdateAsync(turno);
        return MapToDto(updated);
    }

    public async Task<ProximoDisponibleResponseDto> GetProximoTurnoDisponibleAsync(
        ProximoDisponibleRequestDto dto)
    {
        _ = await _profesionalRepository.GetByIdAsync(dto.ProfesionalId)
            ?? throw new NotFoundException("Profesional", dto.ProfesionalId);

        // Si no se especifica FechaDesde, buscar desde ahora
        var fechaDesde = dto.FechaDesde ?? DateTime.Now;

        var resultado = await _turnoRepository.GetProximoTurnoDisponibleAsync(
            dto.ProfesionalId, dto.DuracionMinutos, fechaDesde);

        return resultado
            ?? throw new InvalidOperationException("No se pudo calcular el próximo turno disponible.");
    }

    private static TurnoResponseDto MapToDto(Turno t) => new()
    {
        Id              = t.Id,
        ClienteId       = t.ClienteId,
        ProfesionalId   = t.ProfesionalId,
        FechaHoraInicio = t.FechaHoraInicio,
        FechaHoraFin    = t.FechaHoraFin,
        Estado          = t.Estado.ToString()
    };
}
