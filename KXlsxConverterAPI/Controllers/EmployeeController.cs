using KXlsxConverterAPI.Helpers;
using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using KXlsxConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

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
    [Route("{id}")]
    public IActionResult ViewEmployeeById(int id)
    {
        var employee = _service.GetEmployeeById(id);
        return employee == null ? NotFound() : Ok(employee);
    }
    [HttpGet]
    [Route("{division}/{storeNumber}")]
    public IActionResult ViewAllEmployeesByDivisionAndStore(int division, int storeNumber)
    {
        var employees = _service.GetAllByDivisionAndStoreNumber(division, storeNumber);
        return employees == null ? NotFound() : Ok(employees);
    }
    [HttpGet]
    public IActionResult ViewAllEmployees()
    {
        var employees = _service.GetAllEmployees();
        return employees == null ? NotFound() : Ok(employees);
    }
    //[HttpPost]
    //public IActionResult PostEmployee(Employee employee)
    //{
    //    try
    //    {
    //        _service.AddEmployee(employee);
    //        return Created();
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.Error.WriteLine(ex);
    //        return BadRequest("Error with employee format");
    //    }
    //}
    [HttpPost]
    public IActionResult PostEmployeeToStore(Employee employee)
    {
        try
        {
            _service.AddEmployee(employee);
            return Ok(employee);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return BadRequest("Error with employee format");
        }
    }
    [HttpPost]
    [Route("Bulk")]
    public IActionResult PostMultipleEmployees(List<Employee> employees)
    {
        try
        {
            _service.AddEmployeeBatch(employees);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return BadRequest("Error with employee format");
        }
    }
    [HttpPatch]
    public IActionResult PatchEmployee(Employee employee)
    {
        try
        {
            _service.UpdateEmployee(employee);
            return Ok(employee);
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
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file empty");
            }
            if (!string.Equals(System.IO.Path.GetExtension(file.FileName), ".xlsx"))
            {
                return BadRequest($"Wrong file type uploaded, {System.IO.Path.GetExtension(file.FileName)} not accepted");
            }

            var allEmployees = _service.GetAllByDivisionAndStoreNumber(division, storeNumber); // Ok if empty
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
                    fixedSchedule = converter.ConvertXlsx();
                }


            }

            return Ok(fixedSchedule);
        }
        catch (Exception ex)
        { 
            return BadRequest(ex.Message);
        }
        
    }
    [HttpDelete]
    public IActionResult DeleteEmployee(Employee employee)
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
    [HttpDelete]
    [Route("All")]
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
    [HttpDelete]
    [Route("{division}/{storeNumber}")]
    public IActionResult DeleteAllByDivisionAndStoreNumber(int division, int storeNumber)
    {
        try
        {
            _service.DeleteAllByDivisionAndStoreNumber(division, storeNumber);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"{ex.Message}; Employee not found, unable to delete");
        }
    }
}
