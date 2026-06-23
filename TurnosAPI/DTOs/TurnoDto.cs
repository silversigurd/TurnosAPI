using System.ComponentModel.DataAnnotations;

namespace TurnosAPI.DTOs;

public class CreateTurnoDto
{
    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int ProfesionalId { get; set; }

    [Required]
    public DateTime FechaHoraInicio { get; set; }

    [Required]
    public DateTime FechaHoraFin { get; set; }
}

// El estado se devuelve como string para que el cliente no necesite conocer los valores del enum
public class TurnoResponseDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProfesionalId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Estado { get; set; } = string.Empty;
}

// Resultado de sp_TurnosPorCliente — incluye el nombre del profesional vía JOIN en SQL
public class TurnoConProfesionalDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProfesionalId { get; set; }
    public string NombreProfesional { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Estado { get; set; } = string.Empty;
}
