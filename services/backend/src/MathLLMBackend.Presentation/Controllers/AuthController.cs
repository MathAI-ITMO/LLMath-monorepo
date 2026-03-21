using MathLLMBackend.Core.Services.AuthService;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Dtos.Auth;
using MathLLMBackend.Presentation.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _authService.RegisterAsync(
                    registerDto.Email,
                    registerDto.Password,
                    registerDto.FirstName,
                    registerDto.LastName,
                    registerDto.StudentGroup,
                    ct);

                return Ok(new UserInfoDto(
                    Guid.Parse(user.Id),
                    user.Email ?? string.Empty,
                    user.FirstName,
                    user.LastName,
                    user.StudentGroup,
                    Role.User));
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = ex.Message;
                
                return BadRequest(new 
                {
                    errors = new Dictionary<string, string[]>
                    {
                        { "", new[] { errorMessage } }
                    },
                    title = "Ошибка регистрации",
                    status = StatusCodes.Status400BadRequest
                });
            }
        }

        [HttpGet("validate")]
        [AllowAnonymous]
        public IActionResult Validate()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var firstName = User.FindFirst(ClaimTypeConstants.FirstName)?.Value ?? string.Empty;
            var lastName = User.FindFirst(ClaimTypeConstants.LastName)?.Value ?? string.Empty;
            var studentGroup = User.FindFirst(ClaimTypeConstants.StudentGroup)?.Value ?? string.Empty;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            return Ok(new UserInfoDto(
                userGuid,
                email,
                firstName,
                lastName,
                studentGroup,
                role));
        }
    }
} 