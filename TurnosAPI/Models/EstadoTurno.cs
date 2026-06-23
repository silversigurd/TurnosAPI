namespace TurnosAPI.Models;

// El estado se guarda como INT en la base de datos.
// Cancelado no borra el registro — soft delete para mantener historial.
public enum EstadoTurno
{
    Pendiente  = 0,
    Confirmado = 1,
    Cancelado  = 2
}
