using Microsoft.Data.SqlClient;
using System.Data;
using TurnosAPI.Data;
using TurnosAPI.DTOs;
using TurnosAPI.Models;

namespace TurnosAPI.Repositories;

// EF Core para el CRUD simple, ADO.NET directo para los stored procedures.
// Con ADO.NET proyectamos directamente al DTO sin cargar objetos relacionados innecesarios.
public class TurnoRepository : ITurnoRepository
{
    private readonly TurnosDbContext _context;
    private readonly string _connectionString;

    public TurnoRepository(TurnosDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La connection string 'DefaultConnection' no está configurada.");
    }

    public async Task<Turno?> GetByIdAsync(int id)
        => await _context.Turnos.FindAsync(id);

    public async Task<Turno> CreateAsync(Turno turno)
    {
        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();
        return turno;
    }

    public async Task<Turno> UpdateAsync(Turno turno)
    {
        _context.Turnos.Update(turno);
        await _context.SaveChangesAsync();
        return turno;
    }

    // Llama a sp_ValidarSuperposicion — retorna true si hay conflicto de horario
    public async Task<bool> HaySuperposicionAsync(
        int profesionalId, DateTime inicio, DateTime fin, int? excluirTurnoId = null)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("sp_ValidarSuperposicion", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ProfesionalId", profesionalId);
        command.Parameters.AddWithValue("@FechaHoraInicio", inicio);
        command.Parameters.AddWithValue("@FechaHoraFin", fin);
        // ADO.NET no acepta null directamente como valor de parámetro SQL
        command.Parameters.AddWithValue("@ExcluirTurnoId", (object?)excluirTurnoId ?? DBNull.Value);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // Llama a sp_TurnosPorCliente y mapea el resultado al DTO
    public async Task<IEnumerable<TurnoConProfesionalDto>> GetTurnosPorClienteAsync(int clienteId)
    {
        var turnos = new List<TurnoConProfesionalDto>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("sp_TurnosPorCliente", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ClienteId", clienteId);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            turnos.Add(new TurnoConProfesionalDto
            {
                Id                = reader.GetInt32(reader.GetOrdinal("Id")),
                ClienteId         = reader.GetInt32(reader.GetOrdinal("ClienteId")),
                ProfesionalId     = reader.GetInt32(reader.GetOrdinal("ProfesionalId")),
                NombreProfesional  = reader.GetString(reader.GetOrdinal("NombreProfesional")),
                FechaHoraInicio   = reader.GetDateTime(reader.GetOrdinal("FechaHoraInicio")),
                FechaHoraFin      = reader.GetDateTime(reader.GetOrdinal("FechaHoraFin")),
                Estado            = reader.GetString(reader.GetOrdinal("Estado"))
            });
        }

        return turnos;
    }

    // Llama a sp_ProximoTurnoDisponible y retorna el slot libre encontrado
    public async Task<ProximoDisponibleResponseDto?> GetProximoTurnoDisponibleAsync(
        int profesionalId, int duracionMinutos, DateTime fechaDesde)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("sp_ProximoTurnoDisponible", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ProfesionalId", profesionalId);
        command.Parameters.AddWithValue("@DuracionMinutos", duracionMinutos);
        command.Parameters.AddWithValue("@FechaDesde", fechaDesde);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new ProximoDisponibleResponseDto
            {
                ProfesionalId   = profesionalId,
                FechaHoraInicio = reader.GetDateTime(reader.GetOrdinal("FechaHoraInicio")),
                FechaHoraFin    = reader.GetDateTime(reader.GetOrdinal("FechaHoraFin")),
                DuracionMinutos = duracionMinutos
            };
        }

        return null;
    }
}
