using MathLLMBackend.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace MathLLMBackend.Presentation;

public class JwtTokenHelper
{
    private readonly IConfiguration _config;

    public JwtTokenHelper(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwtToken(User user, DateTime expires)
    {
        var jwtConfig = _config.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        // if (user.FirstName is not null)
        // {
        //     claims.Add(new Claim("FirstName", user.FirstName));
        // }

        // if (user.LastName is not null)
        // {
        //     claims.Add(new Claim("LastName", user.LastName));
        // }

        // if (user.IsuId is not null)
        // {
        //     claims.Add(new Claim("IsuId", user.IsuId?.ToString() ?? ""));
        // }


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
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtConfig = _config.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);
        
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig["Issuer"],
                ValidAudience = jwtConfig["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception)
        {
            throw new UnauthorizedAccessException("Invalid Token");
        }
    }
}
