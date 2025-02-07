using System.Security.Claims;
using MathLLMBackend.DomainServices.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtTokenHelper _jwtTokenHelper;

        public UserController(IUserService userService, JwtTokenHelper jwtTokenHelper)
        {
            _userService = userService;
            _jwtTokenHelper = jwtTokenHelper;
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMeAsync(CancellationToken ct)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _userService.GetById(userId, ct);
            if (user is null)
            {
                return Unauthorized();
            }

            var identity = await _userService.GetIdentity(user, ct);
            return Ok(new { 
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = identity.Email
             });

        }
    }
}
