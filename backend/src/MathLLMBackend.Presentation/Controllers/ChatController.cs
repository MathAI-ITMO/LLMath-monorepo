using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.DomainServices.ChatService;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.Presentation.Helpers;
using MathLLMBackend.Presentation.Dtos.Chats;
using MathLLMBackend.Presentation.Jwt;

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
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto dto, CancellationToken ct)
        {
            var userId =  User.GetUserId();
            var chat = new Chat(dto.Name, userId);
            await _chatService.Create(chat, ct);
            return Ok(
                new ChatDto(chat.Id, chat.Name)
                );
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
