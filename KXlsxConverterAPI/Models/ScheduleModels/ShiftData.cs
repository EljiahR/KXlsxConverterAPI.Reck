namespace KXlsxConverterAPI.Models.ScheduleModels;

public class ShiftData(DateTime start, DateTime end, JobPosition jobPosition, DateTime? subStart = null, DateTime? subEnd = null, string subJobName = "")
{
    public DateTime Start = start;
    public DateTime End = end;
    public JobPosition Position = jobPosition;
    public DateTime? SubStart = subStart;
    public DateTime? SubEnd = subEnd;
    public string SubJobName = subJobName;
}