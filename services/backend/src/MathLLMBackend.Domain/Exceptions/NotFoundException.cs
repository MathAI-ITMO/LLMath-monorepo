namespace MathLLMBackend.Domain.Exceptions;

public class NotFoundException(string message, Exception? inner = null)
    : InvalidOperationException(message, inner);