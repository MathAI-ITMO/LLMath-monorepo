using MathLLMBackend.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace MathLLMBackend.Presentation.Jwt;

public class JwtTokenHelper
{
    private readonly IConfiguration _config;

    public JwtTokenHelper(IConfiguration config)
    {
        _config = config;
    }

    public Jwt GenerateJwtToken(User user, DateTime expires)
    {
        var jwtConfig = _config.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = jwtConfig["Issuer"],
            Audience = jwtConfig["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new Jwt(tokenHandler.WriteToken(token), expires);
    }
}
