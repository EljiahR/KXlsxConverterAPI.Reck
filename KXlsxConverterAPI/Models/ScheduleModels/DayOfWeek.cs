namespace KXlsxConverterAPI.Models.ScheduleModels;

public class DayOfWeek
{
    public string Day { get; set; } // Sunday, Monday, etc..
    public DateTime Date { get; set; }
    public List<JobPosition> JobPositions { get; set; }
}
