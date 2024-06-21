using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers
{
    public class XlsxConverter
    {
        private static Regex dayOfWeekRegex = new Regex(@"^[A-Z]{1}[a-z]{2}\s\d{2}\/\d{2}\/\d{4}$");
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
                for (int row = 1; row <= rowCount; row++)
                {
                    string? firstCell = ws.Cells[row, 1].Value?.ToString();
                    if (!string.IsNullOrEmpty(firstCell) && dayOfWeekRegex.IsMatch(firstCell))
                    {
                        DateTime newWeekday = DateTime.Parse(firstCell);
                        days.Add(new WeekdaySchedule(newWeekday.ToString("dddd"), newWeekday));
                    }
                }
            }
            
            
            
            days.Add(new WeekdaySchedule("Testday", DateTime.Today));

            return days;
        }
    }
}
