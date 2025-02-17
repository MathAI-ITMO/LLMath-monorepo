using System;

namespace MathLLMBackend.Presentation.Jwt;

public record Jwt(string Token, DateTime ValidUntill);
