namespace TurnosAPI.Exceptions;

// → 404 Not Found
public class NotFoundException : Exception
{
    public NotFoundException(string entidad, int id)
        : base($"{entidad} con Id {id} no fue encontrado/a.") { }

    public NotFoundException(string message)
        : base(message) { }
}
