using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Domain.Entities;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using MathLLMBackend.Domain.Enums;
    using MathLLMBackend.Presentation.Dtos.Messages;
    using Microsoft.AspNetCore.Identity;

    namespace MathLLMBackend.Presentation.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class MessageController : ControllerBase
        {
            private readonly IChatService _service;
            private readonly ILogger<MessageController> _logger;
            private readonly UserManager<IdentityUser> _userManager;

            public MessageController(IChatService service, ILogger<MessageController> logger, UserManager<IdentityUser> userManager)
            {
                _service = service;
                _logger = logger;
                _userManager = userManager;
            }
    
            [HttpPost("complete")]
            [Authorize]
            public async Task Complete([FromBody] MessageCreateDto dto, CancellationToken ct)
            {
                var userId = _userManager.GetUserId(User);
                if (userId is null)
                {
                    Response.StatusCode = 401;
                    return;
                }

                var chat = await _service.GetChatById(dto.ChatId, ct);

                if (chat is null)
                {
                    Response.StatusCode = 400;
                    return;
                }

                if (chat.UserId != userId)
                {
                    Response.StatusCode = 401;
                    return;
                }
                
                var message = new Message(chat, dto.Text, MessageType.User);
                var response = _service.CreateMessage(message, ct);

                var outputStream = Response.Body;

                await using var writer = new StreamWriter(outputStream);

                await foreach (var messageLine in response)
                {
                    await writer.WriteAsync(messageLine);
                    await writer.FlushAsync(ct);
                }
            }
    
            [HttpGet("get-messages-from-chat")]
            [Authorize]
            public async Task<IActionResult> GetAllMessagesFromChat(Guid chatId, CancellationToken ct)
            {
                var userId  = _userManager.GetUserId(User);
                if (userId is null)
                {
                    return Unauthorized();
                }

                var chat = await _service.GetChatById(chatId, ct);
                if (chat is null)
                {
                    return BadRequest();
                }

                if (chat.UserId != userId)
                    return Unauthorized();
                
                var messages = await _service.GetAllMessageFromChat(chat, ct);
                
                return Ok(
                    messages.Select(m => new MessageDto(m.Id, m.ChatId, m.Text, m.MessageType.ToString(), m.CreatedAt))
                );
            }
        }
    }
