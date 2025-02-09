namespace KXlsxConverterAPI.Models.ScheduleModels;

public class ShiftData(DateTime start, DateTime end, JobPosition jobPosition, bool isSubshift = false)
{
    public DateTime Start = start;
    public DateTime End = end;
    public JobPosition Position = jobPosition;
    public bool IsSubshift = isSubshift;
}