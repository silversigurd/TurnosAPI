namespace TurnosAPI.Exceptions;

// → 409 Conflict (el horario del profesional ya está ocupado, no es un dato inválido)
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message) { }
}
