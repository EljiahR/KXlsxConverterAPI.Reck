using KXlsxConverterAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KXlsxConverterAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    
    // Don't @ me, I know
    private readonly string username;
    private readonly string password;

    public AuthController(IConfiguration config)
    {
        _config = config;
        username = string.IsNullOrEmpty(config["USERNAME"]) ? "test" : config["USERNAME"]!;
        password = string.IsNullOrEmpty(config["PASSWORD"]) ? "test" : config["PASSWORD"]!;
    }

    [HttpPost]
    [Route("login")]
    public IActionResult Login([FromBody] UserLogin model)
    {
        if (model.Username == username && model.Password == password)
        {
            var token = GenerateJwtToken(model.Username);
            return Ok(new { token });
        }
        return Unauthorized();
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(_config["Issuer"],
            _config["Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(90),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
