using KXlsxConverterAPI.Helpers;
using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using KXlsxConverterAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace KXlsxConverterAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "AdminOrBelongsToStore")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;
    private readonly UserManager<EmployeeUser> _userManager;
    public EmployeeController(IEmployeeService service, UserManager<EmployeeUser> userManager)
    {
        _service = service;
        _userManager= userManager;
    }

    // GET: /Employee/1
    [HttpGet]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ViewEmployeeByIdAsync(int id)
    {
        var employee = await _service.GetEmployeeByIdAsync(id);
        return employee == null ? NotFound() : Ok(employee);
    }

    // GET: /Employee/16/549
    [HttpGet]
    [Route("{division}/{storeNumber}")]
    public async Task<IActionResult> ViewAllEmployeesByDivisionAndStoreAsync(int division, int storeNumber)
    {
        if (IsAdminOrMatchesDivisionAndStore(division, storeNumber))
        {
            var employees = await _service.GetAllByDivisionAndStoreNumberAsync(division, storeNumber);
            return employees == null ? NotFound() : Ok(employees);
        }
        
        return Unauthorized(new { message = "You do not have access to this store"});
    }

    // GET: /Employee
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ViewAllEmployeesAsync()
    {
        var employees = await _service.GetAllEmployeesAsync();
        return employees == null ? NotFound() : Ok(employees);
    }

    // POST: /Employee
    [HttpPost]
    public async Task<IActionResult> PostEmployeeToStoreAsync(Employee employee)
    {
        if (IsAdminOrMatchesDivisionAndStore(employee.Division, employee.StoreNumber))
        {
            try
            {
                if(employee.Birthday != null)
                {
                    employee.Birthday = DateTime.SpecifyKind(employee.Birthday.Value, DateTimeKind.Utc);
                }
                await _service.AddEmployeeAsync(employee);
                return Ok(employee);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return BadRequest("Error with employee format");
            }
        }
        return Unauthorized(new { message = "You do not have access to this store"});
    }

    // POST: /Employee/Bulk
    [HttpPost]
    [Route("Bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostMultipleEmployeesAsync(List<Employee> employees)
    {
        try
        {
            await _service.AddEmployeeBatchAsync(employees);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return BadRequest("Error with employee format");
        }
    }

    // PATCH: /Employee
    [HttpPatch]
    public IActionResult PatchEmployee(Employee employee)
    {
        if (IsAdminOrMatchesDivisionAndStore(employee.Division, employee.StoreNumber))
        {
            try
            {
                if (employee.Birthday != null) employee.Birthday = DateTime.SpecifyKind(employee.Birthday.Value, DateTimeKind.Utc);
                _service.UpdateEmployee(employee);
                return Ok(employee);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return BadRequest($"{ex.Message}: {ex.InnerException}");
            }
        }
        
        return Unauthorized(new { message = "You do not have access to this employee"});
    }

    // POST: /Employee/Dailies/16/549
    [HttpPost]
    [Route("Dailies/{division}/{storeNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> PostSchedule(IFormFile file, int division, int storeNumber)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file empty");
            }
            if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx"))
            {
                return BadRequest($"Wrong file type uploaded, {Path.GetExtension(file.FileName)} not accepted");
            }

            var allEmployees = await _service.GetAllByDivisionAndStoreNumberAsync(division, storeNumber); // Ok if empty
            List<WeekdaySchedule> fixedSchedule = new();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];
                    if (ws == null) throw new NullReferenceException("No usable worksheet was found");
                    XlsxConverter converter = new XlsxConverter(allEmployees, ws);
                    fixedSchedule = await converter.ConvertXlsx();
                }


            }

            return Ok(fixedSchedule);
        }
        catch (Exception ex)
        { 
            return BadRequest(ex.Message);
        }
        
    }

    // DELETE /Employee/1
    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteEmployeeAsync(int id)
    {
        var employee = await _service.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { message = "User not found" });
        }
        else if (IsAdminOrMatchesDivisionAndStore(employee.Division, employee.StoreNumber))
        {
            try
            {
                _service.DeleteEmployee(employee);
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}; Employee not found, unable to delete");
            }
        }
        return Unauthorized(new { message = "You do not have access to this employee"});
        
    }

    // DELETE /Employee/All
    [HttpDelete]
    [Route("All")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteAllEmployees()
    {
        try
        {
            _service.DeleteAllEmployees();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"{ex.Message}; Employee not found, unable to delete");
        }
    }

    //DELETE /Employee/16/549
    [HttpDelete]
    [Route("{division}/{storeNumber}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllByDivisionAndStoreNumber(int division, int storeNumber)
    {
        try
        {
            await _service.DeleteAllByDivisionAndStoreNumberAsync(division, storeNumber);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"{ex.Message}; Employee not found, unable to delete");
        }
    }

    public bool IsAdminOrMatchesDivisionAndStore(int division, int store)
    {
        var user = User;
        if (user.IsInRole("Admin")) return true;
        
        var userDivisions = user.Claims.Where(x => x.Type == "DivisionNumber").Select(x => x.Value).ToList();
        var userStores = user.Claims.Where(x => x.Type == "StoreNumber").Select(x => x.Value).ToList();

        if (userDivisions.Contains(division.ToString()) && userStores.Contains(store.ToString()))
        {
            return true;
        }
        
        return false;
    }
}
