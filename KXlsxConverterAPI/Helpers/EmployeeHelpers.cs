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

        // Im sure theres a more dynamic way of getting these, just seems like too much work for basically the same payoff
        // Im sure the order of the if else statements are a little weird but it honestly felt the most efficient

        // 8.5 hours is pretty much guarenteed to be a minor with a full length shift
        if(shiftLength.Hours >= 8.5)
        {
            break1 = startTime.AddHours(2);
            lunch = startTime.AddHours(4);
            break2 = startTime.AddHours(6.5);
        } else if(shiftLength.Hours >= 6.5 && getsLunch)
        {
            break1 = startTime.AddHours(2);
            lunch = startTime.AddHours(4);
            if(shiftLength.Hours < 8) break2 = endTime.AddHours(-1);
            else break2 = endTime.AddHours(-2);
        } else if(shiftLength.Hours >= 8)
        {
            if (breakPreference > 1)
            {
                break1 = startTime.AddHours(3);
                break2 = startTime.AddHours(6);
            }
            else lunch = startTime.AddHours(4);
        } else if(shiftLength.Hours >= 5 && getsLunch)
        {
            break1 = startTime.AddHours(2);
            lunch = startTime.AddHours(4);
        } else if(shiftLength.Hours >= 6)
        {
            if(breakPreference > 1)
            {
                // This section is subject to change
                break2 = endTime.AddHours(-2);
                if (break2.GetValueOrDefault().AddHours(-3) >= startTime.AddHours(2)) break1 = break2.GetValueOrDefault().AddHours(-3);
                else break1 = startTime.AddHours(2);
            } else
            {
                lunch = startTime.AddHours(shiftLength.Hours / 2);
            }
        } else
        {
            // Only shifts left should be getting exactly one 15 minute break
            break1 = startTime.AddHours(shiftLength.Hours / 2);
        }

        return (break1, lunch, break2);
    }
}
