namespace KXlsxConverterAPI.Helpers;

public class StringFixer
{
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
}
