namespace TurnosAPI.Models;

// Genérico a propósito: puede ser un médico, mecánico, estilista, etc.
public class Profesional
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
}
