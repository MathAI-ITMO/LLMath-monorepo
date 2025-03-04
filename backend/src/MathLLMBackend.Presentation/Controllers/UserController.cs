using System.Security.Claims;
using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.Presentation.Dtos.Common;
using MathLLMBackend.Presentation.Helpers;
using MathLLMBackend.Presentation.Jwt;
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
        [Produces("application/json", Type = typeof(UserInfoDto))]
        [Authorize]
        public IActionResult GetMe(CancellationToken ct)
        {
            var user = User.GetUser();
            return Ok(
                new UserInfoDto(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName));;

        }
    }
}
