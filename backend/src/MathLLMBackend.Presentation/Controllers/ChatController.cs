using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.Presentation.Dtos.Chats;
using Microsoft.AspNetCore.Identity;
using MathLLMBackend.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public ChatController(IChatService chatService, ILogger<ChatController> logger, UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _chatService = chatService;
            _logger = logger;
            _userManager = userManager;
            _context = context;
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
            
            // TODO: refactor move logic to service
            var chat = new Chat(dto.Name, userId);

            if (dto.ProblemHash is null)
            {
                await _chatService.Create(chat, ct);
            }
            else
            {
                await _chatService.Create(chat, dto.ProblemHash, 0, ct);
            }
            
            return Ok(
                new ChatDto(chat.Id, chat.Name, chat.Type.ToString(), null)
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
            return Ok(chats.Select(c => new ChatDto(c.Id, c.Name, c.Type?.ToString() ?? "Chat", null)).ToList());
        }

        [HttpGet("get/{chatId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetChatDetails(Guid chatId, CancellationToken ct)
        {
            var chat = await _chatService.GetChatById(chatId, ct);
            if (chat == null)
            {
                _logger.LogWarning("Chat with ID {ChatId} not found when trying to get details.", chatId);
                return NotFound();
            }

            int? taskType = null;
            if (chat.Type == ChatType.ProblemSolver)
            {
                var userTask = await _context.UserTasks
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(ut => ut.AssociatedChatId == chatId, ct);
                if (userTask != null)
                {
                    taskType = userTask.TaskType;
                }
            }

            return Ok(new ChatDto(chat.Id, chat.Name, chat.Type?.ToString() ?? "Chat", taskType));
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
