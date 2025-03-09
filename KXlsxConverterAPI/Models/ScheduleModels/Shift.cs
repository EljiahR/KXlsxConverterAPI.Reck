namespace KXlsxConverterAPI.Models.ScheduleModels;

public class Shift
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string BaggerName {  get; set; } = string.Empty;
    public DateTime ShiftStart { get; set; }
    public DateTime ShiftEnd { get; set; }
    public DateTime? BreakOne { get; set; } = null;
    public DateTime? Lunch { get; set; } = null;
    public DateTime? BreakTwo { get; set; } = null;
    public string OriginalPosition { get; set; } = string.Empty;
    public Subshift? Subshift {get; set;}
}
