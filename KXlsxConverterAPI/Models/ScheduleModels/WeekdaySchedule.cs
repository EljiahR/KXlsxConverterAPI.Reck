namespace KXlsxConverterAPI.Models.ScheduleModels;

public class WeekdaySchedule
{
    public string Day { get; set; } // Sunday, Monday, etc..
    public DateTime Date { get; set; }
    public List<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
    public CartSlot[] Carts { get; set; } // 36 slots, starting at 6:00a + 30m until 11:30p
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public WeekdaySchedule(string day, DateTime date)
    {
        Day = day;
        Date = date;
        Carts = Enumerable.Range(0, 36)
                    .Select(c => new CartSlot(Date.AddHours(6 + c * 0.5)))
                    .ToArray();
    }
}
