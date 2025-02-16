using System.Security.Claims;
using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.DomainServices.ChatService;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Presentation.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly ILogger<AuthController> _logger;
        public ChatController(IUserService userService, IChatService chatService, JwtTokenHelper jwtTokenHelper, ILogger<AuthController> logger)
        {
            _userService = userService;
            _chatService = chatService;
            _jwtTokenHelper = jwtTokenHelper;
            _logger = logger;
        }

        [HttpPost("create-chat")]
        [Authorize]
        public async Task<IActionResult> CreateChat([FromBody] ChatNameDto dto, CancellationToken ct)
        {
            var existingToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(existingToken))
                return Unauthorized();

            try
            {
                var principal = _jwtTokenHelper.ValidateJwtToken(existingToken);
                var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var chat = new Chat(dto.name, userId);
                var registeredChat = await _chatService.Create(chat, ct);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while trying to renew token {exception}", ex.Message);
                return Unauthorized();
            }
        }



    }
}
