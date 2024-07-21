namespace KXlsxConverterAPI.Models.ScheduleModels;

public class JobPosition
{
    public string Name { get; set; }
    public List<Shift> Shifts { get; set; } = new();
    public JobPosition(string name)
    {
        Name = name;
    }
}
