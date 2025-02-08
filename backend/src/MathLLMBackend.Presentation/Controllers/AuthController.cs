using System.Security.Claims;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.Presentation;
using MathLLMBackend.Presentation.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IConfiguration _config;

    public AuthController(IUserService userService, JwtTokenHelper jwtTokenHelper, IConfiguration config)
    {
        _userService = userService;
        _jwtTokenHelper = jwtTokenHelper;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var user = new User(dto.FirstName, dto.LastName, dto.IsuId);

        var registeredUser = await _userService.Create(user, dto.Email, dto.Password, ct);
        

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
    public async Task<IActionResult> RenewToken(CancellationToken ct)
    {
        var existingToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(existingToken))
            return Unauthorized();

        try
        {
            var principal = _jwtTokenHelper.ValidateJwtToken(existingToken);
            var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userService.GetById(userId, ct);
            if (user is null) 
                return Unauthorized();

            var newToken = _jwtTokenHelper.GenerateJwtToken(user, DateTime.UtcNow.AddDays(1));


            return Ok(new { token = newToken });
        }
        catch (Exception)
        {
            return Unauthorized();
        }
    }
}