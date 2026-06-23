namespace TurnosAPI.Exceptions;

// → 400 Bad Request
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message) { }
}
