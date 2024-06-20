namespace KXlsxConverterAPI.Models.ScheduleModels;

public class DayOfWeek
{
    public string Day { get; set; } // Sunday, Monday, etc..
    public DateTime Date { get; set; }
    public List<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
    public DayOfWeek(string day, DateTime date)
    {
        Day = day;
        Date = date;
    }
}
