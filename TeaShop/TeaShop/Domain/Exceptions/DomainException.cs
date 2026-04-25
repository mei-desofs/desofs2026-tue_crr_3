namespace TeaShop.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class UnauthorizedException(string message = "Unauthorized") : DomainException(message);

public class ForbiddenException(string message = "Forbidden") : DomainException(message);

public class NotFoundException(string message) : DomainException(message);

public class ConflictException(string message) : DomainException(message);