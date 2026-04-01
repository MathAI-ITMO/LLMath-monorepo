using MathLLMBackend.Core.Constants;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Domain.Models;
using MathLLMBackend.Presentation.Binders;
using MathLLMBackend.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MathLLMBackend.Presentation.Dtos.Chats;

namespace MathLLMBackend.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto dto, [FromJwt] JwtUser user, CancellationToken ct)
        {
            var chat = new Chat(dto.Name, user.Id);
            var createdChat = dto.ProblemId == null
                ? await _chatService.Create(chat, ct)
                : await _chatService.Create(chat, dto.ProblemId!.Value, TaskType.Default, ct);

            return Ok(
                new ChatDto(createdChat.Id, createdChat.Name, createdChat.Type?.ToString() ?? ChatConstants.DefaultChatTypeName, null, null)
            );

        }

        [HttpGet("get")]
        [ProducesResponseType(typeof(IEnumerable<ChatDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChats([FromJwt] JwtUser user, CancellationToken ct)
        {
            var chats = await _chatService.GetUserChats(user.Id, ct);
            return Ok(chats.Select(c => new ChatDto(c.Id, c.Name, c.Type?.ToString() ?? ChatConstants.DefaultChatTypeName, null, null)).ToList());
        }

        [HttpGet("get/{chatId:guid}")]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChatDetails(Guid chatId, [FromJwt] JwtUser user, CancellationToken ct)
        {
            var isAdmin = User.IsInRole(Role.Admin);

            Chat chat;
            ChatDetailsModel details;

            if (isAdmin)
            {
                chat = await _chatService.GetChatByIdAsync(chatId, ct)
                    ?? throw new NotFoundException($"Chat with ID {chatId} not found.");
                details = await _chatService.GetChatDetailsForAdminAsync(chatId, ct);
            }
            else
            {
                details = await _chatService.GetChatDetailsAsync(chatId, user.Id, ct);
                chat = await _chatService.GetChatByIdForUser(chatId, user.Id, ct);
            }

            return Ok(new ChatDto(chat.Id, chat.Name, chat.Type?.ToString() ?? ChatConstants.DefaultChatTypeName, details.TaskType, details.TheoryLink));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteChat(Guid id, [FromJwt] JwtUser user, CancellationToken ct)
        {
            await _chatService.Delete(id, user.Id, ct);
            return Ok();
        }
    }
}
