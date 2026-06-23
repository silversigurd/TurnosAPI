namespace TurnosAPI.Models;

public class Turno
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProfesionalId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

    // Navigation properties — nullable porque no siempre se cargan
    public Cliente? Cliente { get; set; }
    public Profesional? Profesional { get; set; }
}
