namespace KXlsxConverterAPI.Models.ScheduleModels;

public class JobKeyTracker(string? jobKey, int jobStartColumn, string? subJobKey = null, int subJobStartColumn = 0)
{
    public string? JobKey = jobKey;
    public int JobStartColumn = jobStartColumn;
    public string? SubJobKey = subJobKey;
    public int SubJobStartColumn = subJobStartColumn;
}