using KXlsxConverterAPI.Helpers;
using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
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
            Console.Error.WriteLine(ex);
            return BadRequest("Error with employee format");
        }
    }

    [HttpPost]
    [Route("Dailies/{division}/{storeNumber}")]
    public async Task<IActionResult> PostSchedule(IFormFile file, int division, int storeNumber)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded or file empty");
        }
        if(!string.Equals(System.IO.Path.GetExtension(file.FileName), ".xlsx"))
        {
            return BadRequest($"Wrong file type uploaded, {System.IO.Path.GetExtension(file.FileName)} not accepted");
        }
        
        var allEmployees = _service.GetAllByDivisionAndStoreNumber(division, storeNumber); // Ok if empty
        List<WeekdaySchedule> fixedSchedule = new();
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            XlsxConverter converter = new XlsxConverter(allEmployees);
            fixedSchedule = converter.ConvertXlsx(stream);
        }

        return Ok(fixedSchedule);
    }
}
