using System.Security.Claims;
using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.Presentation.Helpers;
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
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, JwtTokenHelper jwtTokenHelper, ILogger<UserController> logger)
        {
            _userService = userService;
            _jwtTokenHelper = jwtTokenHelper;
            _logger = logger;
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMeAsync(CancellationToken ct)
        {
            var userId = User.GetUserId();

            var user = await _userService.GetById(userId, ct);
            if (user is null)
            {
                _logger.LogError("User not found with ID: {userId} while trying to get user details.", userId);
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
