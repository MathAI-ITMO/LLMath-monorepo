using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Presentation.Dtos.Auth;
using MathLLMBackend.Presentation.Dtos.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Пользователь с таким email уже существует");
                return BadRequest(new 
                {
                    errors = new Dictionary<string, string[]>
                    {
                        { "", new[] { "Пользователь с таким email уже существует" } }
                    },
                    title = "Ошибка регистрации",
                    status = 400,
                    detail = "Пользователь с таким email уже существует"
                });
            }
            
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                StudentGroup = registerDto.StudentGroup
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with extended profile.");
                return Ok(new UserInfoDto(
                    Guid.Parse(user.Id),
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.StudentGroup));
            }

            var errors = new Dictionary<string, List<string>>();
            
            foreach (var error in result.Errors)
            {
                string errorKey = error.Code;
                string errorMessage;
                
                if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                {
                    errorMessage = "Пользователь с таким email уже существует";
                }
                else if (error.Code.StartsWith("Password"))
                {
                    errorMessage = "Пароль не соответствует требованиям безопасности. Используйте буквы разного регистра, цифры и специальные символы";
                }
                else
                {
                    errorMessage = error.Description;
                }
                
                if (!errors.ContainsKey(errorKey))
                {
                    errors[errorKey] = new List<string>();
                }
                
                errors[errorKey].Add(errorMessage);
            }
            
            return BadRequest(new 
            {
                errors = errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()),
                title = "Ошибка регистрации",
                status = 400,
                detail = "Не удалось создать аккаунт. Пожалуйста, исправьте ошибки и попробуйте снова."
            });
        }
    }
} 