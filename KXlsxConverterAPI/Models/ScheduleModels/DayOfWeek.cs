namespace KXlsxConverterAPI.Models.ScheduleModels;

public class DayOfWeek
{
    public string Day { get; set; } // Sunday, Monday, etc..
    public DateTime Date { get; set; }
    public List<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
    public CartSlot[] Carts { get; set; } = new CartSlot[36]; // 36 slots, starting at 6:00a + 30m until 11:30p
    public DayOfWeek(string day, DateTime date)
    {
        Day = day;
        Date = date;
    }
}
