namespace TeaShop.Domain.Exceptions;

public class DomainException : Exception
{
  

    public DomainException(string message) : base(message) { }

    //  Wrapper
    public DomainException(string message, Exception inner) : base(message, inner) { }
}