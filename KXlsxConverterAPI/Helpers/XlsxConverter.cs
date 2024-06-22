using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using OfficeOpenXml;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers
{
    public class XlsxConverter
    {
        private enum CurrentSection { FoundNewDay, FoundEmployee, FoundEnd, FoundNothing };
        private static Regex dayOfWeekRegex = new Regex(@"^[A-Z]{1}[a-z]{2}\s\d{2}\/\d{2}\/\d{4}$");
        private static Regex nameRegex = new Regex(@"^[a-zA-Z]+,\s[a-zA-Z]+");
        
        private List<WeekdaySchedule> days;
        private Dictionary<int, DateTime> timeIndex;
        private WeekdaySchedule? currentDay;
        private ExcelWorksheet? ws;
        private int rowCount = 0;
        private int colCount = 0;

        private static int nameColumnPos = 2;

        public XlsxConverter()
        {
            days = new();
            timeIndex = new();
        }
        public List<WeekdaySchedule> ConvertXlsx(Stream stream, IEnumerable<Employee> storeEmployees)
        {
            
            using (ExcelPackage package = new ExcelPackage(stream))
            { 
                ws = package.Workbook.Worksheets[0];
                if(ws == null)
                {
                    throw new NullReferenceException("Worksheet is empty");
                }
                rowCount = ws.Dimension.Rows;
                colCount = ws.Dimension.Columns;

                bool dayFound = false; // For tracking when in a valid range for shifts
                WeekdaySchedule? currentDay = null;
                for (int row = 1; row <= rowCount; row++)
                {
                    switch(IdentifyRow(row, dayFound))
                    {
                        case CurrentSection.FoundNewDay:
                            dayFound = true;
                            DateTime newWeekday = DateTime.Parse(ws.Cells[row, 1].Value?.ToString() ?? "");
                            currentDay = new WeekdaySchedule(newWeekday.ToString("dddd"), newWeekday);
                            days.Add(currentDay);
                            
                            // I've had troubles with the columns for the times not being consistant
                            // so I've implemented a method to figure it out for the rest of the class
                            if (days.Count == 1) MapTimeIndexes(row + 1);
                            break;
                        case CurrentSection.FoundEmployee when dayFound:
                            //ParseAndAddEmployee(row);
                            break;
                        case CurrentSection.FoundEnd:
                            dayFound = false;
                            currentDay = null;
                            break;
                    }
                }
            }
            
            
            
            //days.Add(new WeekdaySchedule("Testday", DateTime.Today));

            return days;
        }

        private CurrentSection IdentifyRow(int row, bool dayFound)
        {
            
            if (dayOfWeekRegex.IsMatch(ws.Cells[row, 1].Value?.ToString() ?? ""))
                return CurrentSection.FoundNewDay;
            else if (dayFound && ws.Cells[row, 1].Value?.ToString() == "Forcasted")
                return CurrentSection.FoundEnd;
            else if (dayFound && nameRegex.IsMatch(ws.Cells[row, nameColumnPos].Value?.ToString() ?? ""))
                return CurrentSection.FoundEmployee;
            return CurrentSection.FoundNothing;
        }

        private void ParseAndAddEmployee(int row)
        {
            throw new NotImplementedException();
        }

        private void MapTimeIndexes(int row)
        {
            for (int col = 1; col <= colCount; col++)
            {
                double dateNum;
                if (double.TryParse(ws.Cells[row, col].Value?.ToString(), out dateNum))
                {
                    timeIndex.Add(col, DateTime.FromOADate(dateNum));
                }
            }
        }
    }
}
