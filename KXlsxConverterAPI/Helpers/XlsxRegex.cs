using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers;

public static partial class XlsxRegex {
    [GeneratedRegex(@"^[A-Z]{1}[a-z]{2}\s\d{2}\/\d{2}\/\d{4}$")]
    public static partial Regex DayOfWeek();

    [GeneratedRegex(@"^[a-zA-Z]+,\s[a-zA-Z]+")]
    public static partial Regex Name();
}