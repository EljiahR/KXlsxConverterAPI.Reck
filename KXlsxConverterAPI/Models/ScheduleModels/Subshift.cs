namespace KXlsxConverterAPI.Models.ScheduleModels;

public class Subshift
{
    public DateTime ShiftStart { get; set; }
    public DateTime ShiftEnd { get; set; }
    public string OriginalPosition { get; set; } = string.Empty;

}