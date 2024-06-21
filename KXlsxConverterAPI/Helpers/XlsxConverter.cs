using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers
{
    public class XlsxConverter
    {
        private static Regex dayOfWeekRegex = new Regex(@"^[A-Z]{1}[a-z]{2}\s\d{2}\/\d{2}\/\d{4}$");
        private static Regex nameRegex = new Regex(@"^[a-zA-Z]+,\s[a-zA-Z]+");
        
        private static int nameColumnPos = 2;
        public static List<WeekdaySchedule> ConvertXlsx(Stream stream, IEnumerable<Employee> storeEmployees)
        {
            List<WeekdaySchedule> days = new List<WeekdaySchedule>();
            using (ExcelPackage package = new ExcelPackage(stream))
            { 
                ExcelWorksheet ws = package.Workbook.Worksheets[0];
                if(ws == null)
                {
                    throw new NullReferenceException("Worksheet is empty");
                }
                int rowCount = ws.Dimension.Rows;
                int colCount = ws.Dimension.Columns;

                bool dayFound = false; // For tracking when in a valid range for shifts
                for (int row = 1; row <= rowCount; row++)
                {
                    // First cell in row contains information regarding either current day or "Forcasted" which indicates end of day
                    string? firstCell = ws.Cells[row, 1].Value?.ToString();
                    if (!dayFound && !string.IsNullOrEmpty(firstCell) && dayOfWeekRegex.IsMatch(firstCell))
                    {
                        dayFound = true;
                        DateTime newWeekday = DateTime.Parse(firstCell);
                        days.Add(new WeekdaySchedule(newWeekday.ToString("dddd"), newWeekday));
                    } else if(dayFound && firstCell == "Forcasted")
                    {
                        dayFound = false;
                    }
                    if (dayFound)
                    {
                        string? nameColumnValue = ws.Cells[row, nameColumnPos].Value?.ToString();
                        if(!string.IsNullOrEmpty(nameColumnValue) && nameRegex.IsMatch(nameColumnValue))
                        {

                        }
                    }
                }
            }
            
            
            
            days.Add(new WeekdaySchedule("Testday", DateTime.Today));

            return days;
        }
    }
}
