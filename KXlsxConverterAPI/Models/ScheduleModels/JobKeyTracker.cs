namespace KXlsxConverterAPI.Models.ScheduleModels;

public class JobKeyTracker(string? jobKey, int jobStartColumn, string subJobKey = "", int subJobStartColumn = -1, int subJobEndColumn = -1)
{
    public string? JobKey = jobKey;
    public int JobStartColumn = jobStartColumn;
    public string SubJobKey = subJobKey;
    public int SubJobStartColumn { get; set; } = subJobStartColumn;
    public int SubJobEndColumn { get; set; } = subJobEndColumn;
}