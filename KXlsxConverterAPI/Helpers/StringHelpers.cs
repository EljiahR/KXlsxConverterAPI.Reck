using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers;

public class StringHelpers
{
    private static Regex mcRegex = new(@"^mc", RegexOptions.IgnoreCase);
    public static (string, string) GetFirstAndLastName(string nameCell)
    {
        if (string.IsNullOrEmpty(nameCell)) return ("", "");
        // Cells are formated as Lastname, Firstname M
        // M (middle initial) may or may not be present, but not needed regardless
        string[] names = nameCell.Split(", ");
        string lastName = names[0];
        string firstName = names[1].Split(" ")[0];
        return (firstName, lastName);
    }

    public static string GetProperCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        if (name.Length < 2) return name.ToUpper();
        string result = name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
        if (mcRegex.IsMatch(result))
        {
            if (result.Length == 2) return "Mc";
            if (result.Length == 3) return "Mc" + result.Substring(2).ToUpper();
            result = "Mc" + result.Substring(2, 1).ToUpper() + result.Substring(3).ToLower();
        }
        return result;
    }

    public static bool ContainsOne(string original, IEnumerable<string> stringsToCheck)
    {
        foreach (string s in stringsToCheck)
        {
            if (original.Contains(s)) return true;
        }
        return false;
    }
}
