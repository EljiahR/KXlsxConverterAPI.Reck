using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers;

public class XlsxConverter
{
    private enum CurrentSection { FoundNewDay, FoundEmployee, FoundNothing };
    private static Regex dayOfWeekRegex = new Regex(@"^[A-Z]{1}[a-z]{2}\s\d{2}\/\d{2}\/\d{4}$");
    private static Regex nameRegex = new Regex(@"^[a-zA-Z]+,\s[a-zA-Z]+");

    private List<WeekdaySchedule> days; // New list of days to be built as rows are checked

    //Column numbers are occasionally different due to random extra merged columns, so these next few variables are for fixing that
    private Dictionary<int, DateTime> timeIndex; // Index for what time each column represents
    private int nameColumn = 0;
    // Last three columns may not be necessary depending on how reliable the jobkey is with the time columns
    private int locationColumn = 0;
    private int jobColumn = 0;
    private int startColumn = 0;
    private int endColumn = 0;

    private WeekdaySchedule? currentDay;
    private ExcelWorksheet? ws;
    private int rowCount = 0;
    private int colCount = 0;

    private Shift? bathroomShift = null;
    private int bathroomShiftOrder = -1;

    private IEnumerable<Employee> _storeEmployees;

    public XlsxConverter(IEnumerable<Employee> storeEmployees)
    {
        days = new();
        timeIndex = new();
        _storeEmployees = storeEmployees; // Injecting whatever employee list was found before this converter was created
    }
    public List<WeekdaySchedule> ConvertXlsx(Stream stream)
    {

        using (ExcelPackage package = new ExcelPackage(stream))
        {
            ws = package.Workbook.Worksheets[0];
            if (ws == null)
            {
                throw new NullReferenceException("Worksheet is empty");
            }
            rowCount = ws.Dimension.Rows;
            colCount = ws.Dimension.Columns;

            currentDay = null;
            bool daysFound = false;
            for (int row = 1; row <= rowCount; row++)
                switch (IdentifyRow(row, daysFound))
                {
                    case CurrentSection.FoundNewDay:
                        DateTime newWeekday = DateTime.Parse(ws.Cells[row, 1].Value?.ToString() ?? "");

                        days.Add(currentDay);

                        // I've had troubles with the columns for the times not being consistant
                        // so I've implemented a method to figure it out for the rest of the class
                        if (days.Count == 1)
                        {
                            MapTimeIndexes(row + 1);
                            daysFound = true;
                        }
                        else
                        {
                            EmployeeHelpers.FillCarts(currentDay.Carts, currentDay.JobPositions.Where(x => x.Name == "Front End Courtesy Clerk").FirstOrDefault(), bathroomShift);
                        }

                        currentDay = new WeekdaySchedule(newWeekday.ToString("dddd"), newWeekday);
                        // Reseting bathroom bagger
                        bathroomShift = null;
                        bathroomShiftOrder = -1;
                        break;
                    case CurrentSection.FoundEmployee:
                        ParseEmployeeRow(row);
                        break;

                }
        }

        // Filling carts in for the last day
        EmployeeHelpers.FillCarts(currentDay.Carts, currentDay.JobPositions.Where(x => x.Name == "Front End Courtesy Clerk").FirstOrDefault(), bathroomShift);

        return days;
    }

    private CurrentSection IdentifyRow(int row, bool daysFound)
    {

        if (dayOfWeekRegex.IsMatch(ws.Cells[row, 1].Value?.ToString() ?? ""))
            return CurrentSection.FoundNewDay;
        else if (daysFound && nameRegex.IsMatch(ws.Cells[row, nameColumn].Value?.ToString() ?? ""))
            return CurrentSection.FoundEmployee;
        return CurrentSection.FoundNothing;
    }

    private void ParseEmployeeRow(int row)
    {
        Shift newShift = new Shift();
        string firstName, lastName;

        (firstName, lastName) = StringFixer.GetFirstAndLastName(ws.Cells[row, nameColumn].Value?.ToString());
        // Try to match employee from database here
        Employee? employeePreferences = _storeEmployees
            .Where(e => String.Equals(firstName, e.FirstName, StringComparison.OrdinalIgnoreCase)
                && String.Equals(lastName, e.LastName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        // Creating new employee object for unregistered employees to use default values
        if (employeePreferences == null)
        {
            employeePreferences = new Employee();
            employeePreferences.FirstName = StringFixer.GetProperCase(firstName);
            employeePreferences.LastName = StringFixer.GetProperCase(lastName);
        }

        newShift.FirstName = employeePreferences.FirstName;
        newShift.LastName = employeePreferences.LastName;

        string? jobKey = "";
        string? fillColor;
        int jobStartColumn = 0;
        // Finding beginning of shift  
        for (int col = 1; col <= colCount; col++)
        {
            fillColor = ws.Cells[row, col].Style.Fill.BackgroundColor.Rgb;
            if (fillColor == JobFinder.jobCellFillRgb)
            {
                jobKey = ws.Cells[row, col].Value?.ToString();
                jobStartColumn = col;
                break;
            }
        }
        // Throwing error if jobStartColumn was never found
        if (jobStartColumn == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(jobStartColumn));
        }
        // Plugging in found info


        newShift.ShiftStart = timeIndex[jobStartColumn];


        // Finding end of shift

        int jobEndColumn = 0;
        string? splitJobKey = "";
        for (int col = colCount; col > 1; col--)
        {
            // Actual time ending would be the cell right after the last filled cell, hence col - 1 for all checks
            fillColor = ws.Cells[row, col - 1].Style.Fill.BackgroundColor.Rgb;
            if (fillColor == JobFinder.jobCellFillRgb)
            {
                splitJobKey = ws.Cells[row, col - 1].Value?.ToString();
                jobEndColumn = col;
                break;
            }
        }

        // Throwing error when end is not found, end should always be greater
        if (jobEndColumn <= jobStartColumn)
        {
            throw new ArgumentOutOfRangeException(nameof(jobEndColumn));
        }

        // Fixing any problems caused by nefarious merged time cells
        if (!timeIndex.ContainsKey(jobEndColumn))
        {
            jobEndColumn--;
            if (!timeIndex.ContainsKey(jobEndColumn)) throw new ArgumentOutOfRangeException(nameof(jobEndColumn));
        }
        newShift.ShiftEnd = timeIndex[jobEndColumn];


        // Add shift to existing JobPosition in current day, else create it

        string jobName = string.Empty;
        // File's job key is null as of making this and I could not think of anything other than hardcoding it
        if (string.IsNullOrEmpty(jobKey)) jobName = "File Clerk";
        else if (jobKey == "F") jobName = ws.Cells[row, jobColumn].Value?.ToString();
        else if (JobFinder.jobKeys.ContainsKey(jobKey)) jobName = JobFinder.jobKeys[jobKey];

        var jobPosition = currentDay.JobPositions.Where(j => j.Name == jobName).FirstOrDefault();
        if (jobPosition == null)
        {
            jobPosition = new JobPosition(jobName);
            currentDay.JobPositions.Add(jobPosition);
        }
        jobPosition.Shifts.Add(newShift);

        // Getting breaks for front end employees
        if (jobName.Contains("Front"))
        {
            bool isAdult = employeePreferences.Birthday.HasValue ? (currentDay.Date - employeePreferences.Birthday.GetValueOrDefault()).TotalDays >= 6570 : true;
            (newShift.BreakOne, newShift.Lunch, newShift.BreakTwo) = EmployeeHelpers.GetBreaks(
                newShift.ShiftStart, newShift.ShiftEnd, employeePreferences.PreferredNumberOfBreaks, !isAdult || employeePreferences.GetsLunchAsAdult); // This is so ugly im so sorry
            // Getting the most appropriate bagger for restrooms
            if (jobName.Contains("Courtesy") && newShift.ShiftStart.Hour <= 7 && employeePreferences.BathroomOrder != 0 && (bathroomShiftOrder == -1 || employeePreferences.BathroomOrder < bathroomShiftOrder))
            {
                bathroomShift = newShift;
                bathroomShiftOrder = employeePreferences.BathroomOrder;
            }
        }


    }

    private void MapTimeIndexes(int row)
    {
        bool foundTimes = false;
        bool previousCellWasMerged = false;
        DateTime lastTimeFound = new DateTime(); // Cannot be null due to order of cell checks
        for (int col = 1; col <= colCount; col++)
        {
            // Checking for 15 minute increments after getting to the time columns which are always null
            // Necessary to check row below as each hour label is merged
            if (foundTimes && ws.Cells[row, col].Value is null)
            {
                // Columns are sometimes merged causing unneeded stress trying to working around it
                if (ws.Cells[row + 1, col].Merge)
                {
                    if (previousCellWasMerged)
                    {
                        previousCellWasMerged = false;
                        continue;
                    }
                    else
                    {
                        previousCellWasMerged = true;
                        lastTimeFound = lastTimeFound.AddMinutes(15);
                        timeIndex.Add(col, lastTimeFound);
                    }
                }
                else
                {
                    previousCellWasMerged = false;
                    lastTimeFound = lastTimeFound.AddMinutes(15);
                    timeIndex.Add(col, lastTimeFound);
                }
            }

            if (locationColumn == 0 && ws.Cells[row, col].Value?.ToString() == "Location")
            {
                locationColumn = col;
                continue;
            }
            if (nameColumn == 0 && ws.Cells[row, col].Value?.ToString() == "Name")
            {
                nameColumn = col;
                continue;
            }
            if (jobColumn == 0 && ws.Cells[row, col].Value?.ToString() == "Job")
            {
                jobColumn = col;
                continue;
            }
            if (startColumn == 0 && ws.Cells[row, col].Value?.ToString() == "Start")
            {
                startColumn = col;
                continue;
            }
            if (endColumn == 0 && ws.Cells[row, col].Value?.ToString() == "End")
            {
                endColumn = col;
                continue;
            }

            double dateNum;
            if (double.TryParse(ws.Cells[row, col].Value?.ToString(), out dateNum))
            {
                lastTimeFound = DateTime.FromOADate(dateNum);
                timeIndex.Add(col, lastTimeFound);
                previousCellWasMerged = ws.Cells[row + 1, col].Merge;
            }
            if (!foundTimes && timeIndex.Count > 0) foundTimes = true;
        }
    }
}
