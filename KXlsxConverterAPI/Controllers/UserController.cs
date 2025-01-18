using KXlsxConverterAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KXlsxConverterAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly SignInManager<EmployeeUser> _signInManager;
    private readonly UserManager<EmployeeUser> _userManager;

    public UserController(SignInManager<EmployeeUser> signInManager, UserManager<EmployeeUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        await _signInManager.SignOutAsync();

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.UserName!);
            if (user == null) 
            {
                return Unauthorized(new { message = "Invalid username or password."});
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password!, false, false);
            if (result.Succeeded)
            {
                return Ok(new { message = "Sign in successful!"});
            }
            
            return Unauthorized(new { message = "Invalid username or password."});
        }

        return BadRequest( new { message = "Invalid data."});

    }

    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (ModelState.IsValid)
        {
            var user = new EmployeeUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password!);
            if (result.Succeeded)
            {
                return Ok(new { message = "User created successfully"});
            }

            return BadRequest(result.Errors);
        }

        return BadRequest(new { message = "Invalid data."});
    }
}