using System.ComponentModel.DataAnnotations;

namespace TurnosAPI.DTOs;

public class CreateClienteDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(150)]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string? Email { get; set; }
}

public class UpdateClienteDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(150)]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string? Email { get; set; }
}

public class ClienteResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}
