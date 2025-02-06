using System;

namespace MathLLMBackend.Domain.Exceptions;

public class InvalidPasswordException : DomainException
{
    public InvalidPasswordException() : base("Password is invalid or user does not exist.", null)
    {
    }
}
