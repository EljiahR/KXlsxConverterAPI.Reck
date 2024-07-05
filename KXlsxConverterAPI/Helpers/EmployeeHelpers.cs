namespace KXlsxConverterAPI.Helpers;

public class EmployeeHelpers
{
    public static (DateTime?, DateTime?, DateTime?) GetBreaks(DateTime startTime, DateTime endTime, int breakPreference,bool getsLunch)
    {
        DateTime? break1 = null;
        DateTime? lunch = null;
        DateTime? break2 = null;

        return (break1, lunch, break2);
    }
}
