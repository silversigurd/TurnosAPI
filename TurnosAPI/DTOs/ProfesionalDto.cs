using System.ComponentModel.DataAnnotations;

namespace TurnosAPI.DTOs;

public class CreateProfesionalDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [MaxLength(100, ErrorMessage = "La especialidad no puede superar los 100 caracteres.")]
    public string Especialidad { get; set; } = string.Empty;
}

public class UpdateProfesionalDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [MaxLength(100)]
    public string Especialidad { get; set; } = string.Empty;
}

public class ProfesionalResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
}
