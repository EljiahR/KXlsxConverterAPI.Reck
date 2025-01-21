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
    
    // POST: /User/SignIn
    [HttpPost]
    [Route("SignIn")]
    [AllowAnonymous]
    public async Task<IActionResult> SignInUser([FromBody] LoginDto model)
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

    // POST: /User/Register
    [HttpPost]
    [Route("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterDto model)
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

    // POST: /User/SignOut
    [HttpPost]
    [Route("SignOut")]
    public async Task<IActionResult> SignOutUser()
    {
        await _signInManager.SignOutAsync();

        return Ok(new { message = "Sign out successful!" });
    }

    // Get: /User/Status
    [HttpGet]
    [Route("Status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserSignInStatus()
    {
        if (User == null || User.Identity == null)
        {
            return Unauthorized(new {message = "No user was found"});
        }
        
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            var roleClaims = await _userManager.GetClaimsAsync(user!);

            var storeNumbers = roleClaims.Where(x => x.Type == "StoreNumber")
                                .Select(x => x.Value).ToArray();

            return Ok(new {message = "User is authorized.", authorizedRoutes = new AuthorizedRoutesDto(storeNumbers)});
        }

        return Unauthorized(new {message = "User was not authenticated"});
    }
}