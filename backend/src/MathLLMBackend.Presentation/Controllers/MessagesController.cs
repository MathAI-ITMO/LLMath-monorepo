using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.DomainServices.ChatService;
using MathLLMBackend.DomainServices.MessageService;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.Presentation.Helpers;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Presentation.Dtos.Messages;
using MathLLMBackend.Presentation.Jwt;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly ILogger<AuthController> _logger;
        public MessageController(IUserService userService, IChatService chatService, IMessageService messageService, JwtTokenHelper jwtTokenHelper, ILogger<AuthController> logger)
        {
            _userService = userService;
            _chatService = chatService;
            _messageService = messageService;
            _jwtTokenHelper = jwtTokenHelper;
            _logger = logger;
        }

        [HttpPost("send-message")]
        [Authorize]
        public async Task<IActionResult> CreateChat([FromBody] MessageDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var message = new Message(dto.ChatId, dto.Text, MessageType.User);
            var registeredMessage = await _messageService.Create(message, userId, ct);
            return Ok(
                new MessageDto(registeredMessage.Id, registeredMessage.ChatId, registeredMessage.Text, registeredMessage.MessageType.ToString(), null)
            );
        }

        [HttpGet("get-messages-from-chat")]
        [Authorize]
        public async Task<IActionResult> GetAllMessagesFromChat(long chatId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var messages = await _messageService.GetAllMessageFromChat(userId, chatId, ct);
            return Ok(
                messages.Select(m => new MessageDto(m.Id, m.ChatId, m.Text, m.MessageType.ToString(), m.CreatedAt))
            );
        }
    }
}
