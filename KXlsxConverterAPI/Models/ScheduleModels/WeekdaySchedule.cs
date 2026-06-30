namespace KXlsxConverterAPI.Models.ScheduleModels;

public class WeekdaySchedule
{
    public string Day { get; set; } // Sunday, Monday, etc..
    public DateTime Date { get; set; }
    public List<string> Holidays {get; set;} = new();
    public List<string> Birthdays {get; set;} = new();
    public List<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
    public CartSlot[] Carts { get; set; } // 36 slots, starting at 6:00a + 30m until 11:30p
    public CartSlot[] Carts15 { get; set; } // 54 slots, starting at 9:00a + 15m until 10:30p
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public WeekdaySchedule(string day, DateTime date)
    {
        Day = day;
        Date = date;
        Carts = Enumerable.Range(0, 36)
                    .Select(c => new CartSlot(Date.AddHours(6 + c * 0.5)))
                    .ToArray();
        Carts15 = Enumerable.Range(0, 55)
                    .Select(c => new CartSlot(Date.AddHours(9 + c * 0.25)))
                    .ToArray();
    }
}
