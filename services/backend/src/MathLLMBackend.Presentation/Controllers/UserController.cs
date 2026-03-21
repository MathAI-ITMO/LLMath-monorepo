using MathLLMBackend.Presentation.Binders;
using MathLLMBackend.Presentation.Dtos.Common;
using MathLLMBackend.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUser([FromJwt] JwtUser user)
        {
            return Ok(new UserInfoDto(
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                user.StudentGroup,
                user.Role));
        }
    }
} 