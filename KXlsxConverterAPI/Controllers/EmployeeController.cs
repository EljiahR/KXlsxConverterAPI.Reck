using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KXlsxConverterAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _service;
    public EmployeeController(IEmployeeService service)
    {
        _service = service;
    }
    [HttpGet]
    public IActionResult ViewAllEmployees()
    {
        var employees = _service.GetAllEmployees();
        if (employees == null)
        {
            return NotFound();
        }
        return Ok(employees);
    }
    [HttpPost]
    public IActionResult PostEmployee(Employee employee)
    {
        try
        {
            _service.AddEmployee(employee);
            return Created();
        }
        catch (Exception ex)
        {
            return BadRequest();
        }

    }
}
