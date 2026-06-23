using TurnosAPI.DTOs;
using TurnosAPI.Exceptions;
using TurnosAPI.Models;
using TurnosAPI.Repositories;

namespace TurnosAPI.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClienteResponseDto>> GetAllAsync()
    {
        var clientes = await _repository.GetAllAsync();
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteResponseDto> GetByIdAsync(int id)
    {
        var cliente = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Cliente", id);
        return MapToDto(cliente);
    }

    public async Task<ClienteResponseDto> CreateAsync(CreateClienteDto dto)
    {
        var cliente = new Cliente
        {
            Nombre   = dto.Nombre,
            Telefono = dto.Telefono,
            Email    = dto.Email
        };
        var created = await _repository.CreateAsync(cliente);
        return MapToDto(created);
    }

    public async Task<ClienteResponseDto> UpdateAsync(int id, UpdateClienteDto dto)
    {
        var cliente = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Cliente", id);

        cliente.Nombre   = dto.Nombre;
        cliente.Telefono = dto.Telefono;
        cliente.Email    = dto.Email;

        var updated = await _repository.UpdateAsync(cliente);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var cliente = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Cliente", id);
        await _repository.DeleteAsync(cliente);
    }

    private static ClienteResponseDto MapToDto(Cliente c) => new()
    {
        Id       = c.Id,
        Nombre   = c.Nombre,
        Telefono = c.Telefono,
        Email    = c.Email
    };
}
