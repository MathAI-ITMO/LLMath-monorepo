using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.DomainServices.ChatService;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Presentation.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.Presentation.Helpers;

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
            var userId =  User.GetUserId();
            var chat = new Chat(dto.name, userId);
            await _chatService.Create(chat, ct);
            return Ok();
        }

        [HttpGet("get-chats")]
        [Authorize]
        public async Task<IActionResult> GetChats(CancellationToken ct)
        {
            var userId =  User.GetUserId();
            var chats = await _chatService.GetUserChats(userId, ct);
            return Ok(chats);
        }
    }
}
