using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.Presentation.Dtos.Chats;
using Microsoft.AspNetCore.Identity;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatController(IChatService chatService, ILogger<ChatController> logger, UserManager<IdentityUser> userManager)
        {
            _chatService = chatService;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto dto, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                return Unauthorized();
            }
            
            var chat = new Chat(dto.Name, userId);
            await _chatService.Create(chat, ct);
            return Ok(
                new ChatDto(chat.Id, chat.Name)
                );
        }

        [HttpGet("get")]
        [Authorize]
        public async Task<IActionResult> GetChats(CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                return Unauthorized();
            }
            
            var chats = await _chatService.GetUserChats(userId, ct);
            return Ok(chats.Select(c => new ChatDto(c.Id, c.Name)).ToList());
        }

        [HttpPost("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteChat(Guid id, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                return Unauthorized();
            }
            
            var chat = await _chatService.GetChatById(id, ct);

            if (chat is null)
            {
                return NotFound();
            }
            
            if (chat.User.Id != userId)
            {
                return Unauthorized();
            }
            
            await _chatService.Delete(chat, ct);
            
            return Ok();
        }
    }
}
