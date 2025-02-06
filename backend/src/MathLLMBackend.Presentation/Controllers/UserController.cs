using MathLLMBackend.Domain.Entities;
using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.Presentation.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("new")]
        public async Task CreateUser([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var user = new User(dto.FirstName, dto.LastName, dto.IsuId);

            await _userService.AddUser(user, dto.Email, dto.Password, ct);
        }

        [HttpPost("login")]
        public async Task<TokenDto> LoginUser([FromBody] LoginCredentialsDto dto, CancellationToken ct)
        {
            var session = await _userService.CreateSession(dto.Email, dto.Password, ct);
            return new TokenDto(session.Token);
        }
    }
}
