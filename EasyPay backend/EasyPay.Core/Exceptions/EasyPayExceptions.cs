namespace EasyPay.Core.Exceptions;

public class EasyPayException : Exception
{
    public int StatusCode { get; }

    public EasyPayException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : EasyPayException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.", 404) { }

    public NotFoundException(string message)
        : base(message, 404) { }
}

public class UnauthorizedException : EasyPayException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message, 401) { }
}

public class ForbiddenException : EasyPayException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, 403) { }
}

public class ValidationException : EasyPayException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : base(message, 400)
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { message } } };
    }
}

public class ConflictException : EasyPayException
{
    public ConflictException(string message)
        : base(message, 409) { }
}

public class BusinessRuleException : EasyPayException
{
    public BusinessRuleException(string message)
        : base(message, 422) { }
}
