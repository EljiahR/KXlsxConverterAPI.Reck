using KXlsxConverterAPI.Models.ScheduleModels.Interfaces;

namespace KXlsxConverterAPI.Models.ScheduleModels;

public class JobPosition
{
    public string Name { get; set; }
    public List<IShift> Shifts { get; set; } = new();
    public JobPosition(string name)
    {
        Name = name;
    }
}
