using System.Security.Claims;
using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.DomainServices.ChatService;
using MathLLMBackend.DomainServices.MsgService;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Presentation.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using Microsoft.AspNetCore.Authorization;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MsgController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IMsgService _msgService;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly ILogger<AuthController> _logger;
        public MsgController(IUserService userService, IChatService chatService, IMsgService msgService, JwtTokenHelper jwtTokenHelper, ILogger<AuthController> logger)
        {
            _userService = userService;
            _chatService = chatService;
            _msgService = msgService;
            _jwtTokenHelper = jwtTokenHelper;
            _logger = logger;
        }

        [HttpPost("send-msg")]
        [Authorize]
        public async Task<IActionResult> CreateChat([FromBody] MsgDto dto, CancellationToken ct)
        {
            var existingToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(existingToken))
                return Unauthorized();

            try
            {
                var principal = _jwtTokenHelper.ValidateJwtToken(existingToken);
                var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);                
                var msg = new Msg(dto.chatId, dto.Text, MsgType.user);
                var registeredMsg = await _msgService.Create(msg, dto.chatId, ct);
                return Ok(new {result = $"Message created"});
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while trying to renew token {exception}", ex.Message);
                return Unauthorized();
            }
        }

        [HttpGet("get-messages-from-chat")]
        [Authorize]
        public async Task<IActionResult> GetAllMessagesFromChat(long chatId, CancellationToken ct)
        {
            var existingToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(existingToken))
                return Unauthorized();

            try
            {
                var principal = _jwtTokenHelper.ValidateJwtToken(existingToken);
                var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var msgs = await _msgService.GetAllMsgFromChat(chatId, ct);
                return Ok(new {results = msgs});
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while trying to renew token {exception}", ex.Message);
                return Unauthorized();
            }
        }



    }
}
