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
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(IChatService service, ILogger<MessageController> logger, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _logger = logger;
            _userManager = userManager;
        }
    
        [HttpPost("complete")]
        [Authorize]
        public async Task<IActionResult> Complete([FromBody] MessageCreateDto dto, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                return Unauthorized();
            }

            var chat = await _service.GetChatById(dto.ChatId, ct);
            if (chat is null)
            {
                return BadRequest("Chat not found.");
            }

            if (chat.UserId != userId)
            {
                return Forbid();
            }
            
            var message = new Message(chat, dto.Text, MessageType.User);
            
            string llmResponseText = await _service.CreateMessage(message, ct);

            if (Response.HasStarted)
            {
                _logger.LogWarning("Response has already started before attempting to write LLM response for chat {ChatId}.", dto.ChatId);
                return new EmptyResult();
            }

            if (string.IsNullOrEmpty(llmResponseText))
            {
                _logger.LogWarning("LLM service returned empty or null response for chat {ChatId}, user message: {UserMessage}", dto.ChatId, dto.Text);
                return Ok(string.Empty);
            }
            
            return Ok(llmResponseText);
        }
    
        [HttpGet("get-messages-from-chat")]
        //[Authorize]
        public async Task<IActionResult> GetAllMessagesFromChat(Guid chatId, CancellationToken ct)
        {
            // var userId  = _userManager.GetUserId(User);
            // if (userId is null)
            // {
            //     return Unauthorized();
            // }

            var chat = await _service.GetChatById(chatId, ct);
            if (chat is null)
            {
                return BadRequest();
            }

            // if (chat.UserId != userId)
            //     return Unauthorized();
            
            var messages = await _service.GetAllMessageFromChat(chat, ct);
            
            return Ok(
                messages.Where(m => !m.IsSystemPrompt)
                    .Select(m => new MessageDto(m.Id, m.ChatId, m.Text, m.MessageType.ToString(), m.CreatedAt))
            );
        }
    }
}
