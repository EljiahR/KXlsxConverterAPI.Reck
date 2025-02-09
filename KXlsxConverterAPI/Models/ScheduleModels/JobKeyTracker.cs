namespace KXlsxConverterAPI.Models.ScheduleModels;

public class JobKeyTracker(string? jobKey, int jobStartColumn, string subJobKey = "", int subJobStartColumn = 0, int subJobEndColumn = 0)
{
    public string? JobKey = jobKey;
    public int JobStartColumn = jobStartColumn;
    public string? SubJobKey = subJobKey;
    public int SubJobStartColumn = subJobStartColumn;
    public int SubJobEndColumn {get; set;} = subJobEndColumn;
}