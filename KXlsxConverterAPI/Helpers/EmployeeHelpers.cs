namespace KXlsxConverterAPI.Helpers;

public class EmployeeHelpers
{
    public static (DateTime?, DateTime?, DateTime?) GetBreaks(DateTime startTime, DateTime endTime, int breakPreference,bool getsLunch)
    {
        // Break rules are as follows:
        // All employees get 1 15 minute break if working 4 or more hours
        // All employees get 2 15 or 1 30 minute break if working 6 or more hours
        // Minors are required to take an 30 minute lunch that is optional for adults
        // Minor required lunches are given when working at least 5 hours, I think
        // These 30 minute lunches are not counted towards a shifts length i.e 8.5 hours with a lunch = 8 hours
        // As far as I'm aware, employees taking a lunch cannot take their two 15's together, so 30 minute breaks will be tracked in the same variable

        DateTime? break1 = null;
        DateTime? lunch = null;
        DateTime? break2 = null;
        TimeSpan shiftLength = endTime - startTime;

        if(shiftLength.TotalHours >= 4)
        {
            if(shiftLength.TotalHours < 5)
            {
                break1 = startTime.Add(shiftLength / 2);
            }
            else if(shiftLength.TotalHours >= 5 && getsLunch)
            {
                break1 = startTime.AddHours(2);
                lunch = startTime.AddHours(4);
                shiftLength = shiftLength.Subtract(TimeSpan.FromMinutes(30));
            }
        }

        return (break1, lunch, break2);
    }
}
