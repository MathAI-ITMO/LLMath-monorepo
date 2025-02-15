using System;

namespace MathLLMBackend.Domain.Exceptions;

public class AuthorizationException(string message, Exception? innerException = null)
    : InvalidOperationException(message: message, innerException: innerException);
