using Microsoft.EntityFrameworkCore;
using TurnosAPI.Data;
using TurnosAPI.Models;

namespace TurnosAPI.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly TurnosDbContext _context;

    public ClienteRepository(TurnosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync()
        => await _context.Clientes.ToListAsync();

    public async Task<Cliente?> GetByIdAsync(int id)
        // FindAsync revisa la caché del contexto antes de ir a la base de datos
        => await _context.Clientes.FindAsync(id);

    public async Task<Cliente> CreateAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> UpdateAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task DeleteAsync(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }
}
