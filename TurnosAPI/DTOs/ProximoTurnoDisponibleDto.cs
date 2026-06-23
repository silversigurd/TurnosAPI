using System.ComponentModel.DataAnnotations;

namespace TurnosAPI.DTOs;

public class ProximoDisponibleRequestDto
{
    [Required]
    public int ProfesionalId { get; set; }

    [Required]
    [Range(1, 480, ErrorMessage = "La duración debe estar entre 1 y 480 minutos.")]
    public int DuracionMinutos { get; set; }

    // Si no se especifica, el servicio usa DateTime.Now
    public DateTime? FechaDesde { get; set; }
}

public class ProximoDisponibleResponseDto
{
    public int ProfesionalId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public int DuracionMinutos { get; set; }
}
