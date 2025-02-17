using System.Security.Claims;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.Presentation;
using MathLLMBackend.Presentation.Helpers;
using MathLLMBackend.Presentation.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, JwtTokenHelper jwtTokenHelper, IConfiguration config, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtTokenHelper = jwtTokenHelper;
        _config = config;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var user = new User(dto.FirstName, dto.LastName);

        await _userService.Create(user, dto.Email, dto.Password, ct);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCredentialsDto dto, CancellationToken ct)
    {
        var authenticatedUser = await _userService.AuthenticateUser(dto.Email, dto.Password, ct);
        
        var token = _jwtTokenHelper.GenerateJwtToken(authenticatedUser, DateTime.UtcNow.AddDays(1));

        return Ok(new { token });
    }

    [HttpPost("renew-token")]
    [Authorize]
    public async Task<IActionResult> RenewToken(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var user = await _userService.GetById(userId, ct);
            if (user is null) 
            {
                _logger.LogWarning("User not found while trying to renew token");
                return Unauthorized();
            }

            var newToken = _jwtTokenHelper.GenerateJwtToken(user, DateTime.UtcNow.AddDays(1));

            return Ok(new { token = newToken });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while trying to renew token {exception}", ex.Message);
            return Unauthorized();
        }
    }
}