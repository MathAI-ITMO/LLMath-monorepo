using MathLLMBackend.Presentation.Dtos.Common;

namespace MathLLMBackend.Presentation.Dtos.Auth;

public record LoginResponseDto(TokenDto Token, UserInfoDto User);
