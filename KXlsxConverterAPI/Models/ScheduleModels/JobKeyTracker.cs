namespace KXlsxConverterAPI.Models.ScheduleModels;

public class JobKeyTracker(string? jobKey, int jobStartColumn)
{
    public string? JobKey = jobKey;
    public int JobStartColumn = jobStartColumn;
}