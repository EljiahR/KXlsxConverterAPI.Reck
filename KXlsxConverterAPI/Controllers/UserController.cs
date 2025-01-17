using KXlsxConverterAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KXlsxConverterAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public Task<IActionResult> Login([FromBody] LoginDto model)
    {
        
    }
}