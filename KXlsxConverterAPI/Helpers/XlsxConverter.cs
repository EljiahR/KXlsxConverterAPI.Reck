using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers;

public class XlsxConverter(IEnumerable<Employee> storeEmployees, ExcelWorksheet ws)
{
    private enum CurrentSection { FoundNewDay, FoundEmployee, FoundNothing };
    private static bool IsDayOfWeek(string input) => XlsxRegex.DayOfWeek().IsMatch(input);
    private static bool IsName(string input) => XlsxRegex.Name().IsMatch(input);

    private readonly List<WeekdaySchedule> _days = []; // New list of days to be built as rows are checked

    //Column numbers are occasionally different due to random extra merged columns, so these next few variables are for fixing that
    private readonly Dictionary<int, DateTime> _timeIndex = []; // Index for what time each column represents
    private int _nameColumn = 0;
    // Last three columns may not be necessary depending on how reliable the jobkey is with the time columns
    private int _locationColumn = 0;
    private int _jobColumn = 0;
    private int _startColumn = 0;
    private int _endColumn = 0;

    private WeekdaySchedule? _currentDay;
    private readonly ExcelWorksheet _ws = ws;
    private int _rowCount = 0;
    private int _colCount = 0;

    private Shift? _bathroomShift = null;
    private int _bathroomShiftOrder = -1;

    private readonly IEnumerable<Employee> _storeEmployees = storeEmployees;

    private PublicHoliday[]? _holidays = null;
    public async Task<List<WeekdaySchedule>> ConvertXlsx()
    {
        _rowCount = _ws.Dimension.Rows;
        _colCount = _ws.Dimension.Columns;

        _currentDay = null;
        bool daysFound = false;
        for (int row = 1; row <= _rowCount; row++)
            switch (IdentifyRow(row, daysFound))
            {
                case CurrentSection.FoundNewDay:
                    DateTime newWeekday = DateTime.Parse(_ws.Cells[row, 1].Value?.ToString() ?? "");

                    if (_currentDay != null) _days.Add(_currentDay);

                    // I've had troubles with the columns for the times not being consistant
                    // so I've implemented a method to figure it out for the rest of the class
                    if (_days.Count < 1)
                    {
                        MapTimeIndexes(row + 1);
                        daysFound = true;
                    }
                    else if (_currentDay != null)
                    {
                        var baggerShifts = _currentDay.JobPositions.Where(x => x.Name == "Front End Courtesy Clerk").FirstOrDefault();
                        if (baggerShifts != null) 
                        {
                            EmployeeHelpers.FillCarts(_currentDay.Carts, baggerShifts, _bathroomShift, false);
                            EmployeeHelpers.FillCarts(_currentDay.Carts15, baggerShifts, _bathroomShift, true);
                        }
                    }
                    
                     

                    _currentDay = new WeekdaySchedule(newWeekday.ToString("dddd"), newWeekday);
                    
                    // Initializing holidays
                    if (_holidays == null || _days.Last().Date.ToString("yyyy") != _currentDay.Date.ToString("yyyy")) 
                    {
                        _holidays = await SpecialDayHelpers.GetHolidays(_currentDay.Date.ToString("yyyy"));
                    }
                    // Checking for holidays for current day
                    var holidays = _holidays.Where(h => h.Date == _currentDay.Date).Select(h => h.Name!).ToList();
                    if (holidays.Count > 0) 
                    {
                        _currentDay.Holidays = holidays;
                    }

                    // Checking for birthdays
                    var birthdays = _storeEmployees.Where(e => e.Birthday != null && !e.HideBirthday && e.Birthday.Value.ToString("M") == _currentDay.Date.ToString("M")).Select(e => (string.IsNullOrWhiteSpace(e.PreferredFirstName) ? e.FirstName : e.PreferredFirstName) + " " + e.LastName).ToList();
                    if (birthdays.Count > 0)
                    {
                        _currentDay.Birthdays = birthdays;
                    }

                    // Reseting bathroom bagger
                    _bathroomShift = null;
                    _bathroomShiftOrder = -1;
                    break;
                case CurrentSection.FoundEmployee:
                    try
                    {
                        ParseEmployeeRow(row);
                    }
                    catch (Exception ex)
                    {
                        if (_currentDay != null)
                        {
                            string errorType = ex.GetType().Name;


                            if (_currentDay.Errors.TryGetValue(errorType, out List<String>? errors))
                                errors.Add("Error in row " + row);
                            else
                                _currentDay.Errors.Add(errorType, new(["Error in row " + row]));
                        }

                    }
                    break;

            }


        // Filling carts in for the last day
        if (_currentDay != null)
        {
            var baggerShifts = _currentDay.JobPositions.Where(x => x.Name == "Front End Courtesy Clerk").FirstOrDefault();
            if (baggerShifts != null)
            {
                EmployeeHelpers.FillCarts(_currentDay.Carts, baggerShifts, _bathroomShift, false);
                EmployeeHelpers.FillCarts(_currentDay.Carts15, baggerShifts, _bathroomShift, true);
            } 
                
            _days.Add(_currentDay);
        }
        SortAllShifts();
        return _days;
    }

    private CurrentSection IdentifyRow(int row, bool daysFound)
    {
        if (IsDayOfWeek(_ws.Cells[row, 1].Value?.ToString() ?? ""))
            return CurrentSection.FoundNewDay;
        else if (daysFound && IsName(_ws.Cells[row, _nameColumn].Value?.ToString() ?? ""))
            return CurrentSection.FoundEmployee;
        return CurrentSection.FoundNothing;
    }

    private void ParseEmployeeRow(int row)
    {
        if (_currentDay == null) throw new Exception("currentDay should not be null");

        var shifts = new List<ShiftData>();
        string firstName, lastName;
        var nameCellValue = _ws.Cells[row, _nameColumn].Value?.ToString();

        if (string.IsNullOrWhiteSpace(nameCellValue)) throw new ArgumentNullException($"Name cell at row:{row} column:{_nameColumn} was null");

        (firstName, lastName) = StringHelpers.GetFirstAndLastName(nameCellValue);
        // Try to match employee from database here
        Employee? employeePreferences = _storeEmployees
            .Where(e => String.Equals(firstName, e.FirstName, StringComparison.OrdinalIgnoreCase)
                && String.Equals(lastName, e.LastName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        // Creating new employee object for unregistered employees to use default values
        
         employeePreferences ??= new() {
            FirstName = StringHelpers.GetProperCase(firstName),
            LastName = StringHelpers.GetProperCase(lastName)
        };
        
        if (employeePreferences.PositionOverride == "DELETE")
            return;

        string? fillColor;

        // Finding end of shift

        int jobEndColumn = 0;
        for (int col = _colCount; col > 1; col--)
        {
            // Actual time ending would be the cell right after the last filled cell, hence col - 1 for all checks
            fillColor = _ws.Cells[row, col - 1].Style.Fill.BackgroundColor.Rgb;
            if (fillColor == JobFinder.JobCellFillRgb)
            {
                jobEndColumn = col;
                break;
            }
        }

        // Fixing any problems caused by nefarious merged time cells
        if (!_timeIndex.ContainsKey(jobEndColumn))
        {
            jobEndColumn--;
            if (!_timeIndex.ContainsKey(jobEndColumn)) throw new Exception(nameof(jobEndColumn));
        }
        DateTime wholeShiftEnd = _timeIndex[jobEndColumn];

        List<JobKeyTracker> jobKeys = [];
        int firstJobKeyColumn = 0;

        // Finding beginning of shift and all splits 
        
        for (int col = 1; col <= jobEndColumn; col++)
        {
            fillColor = _ws.Cells[row, col].Style.Fill.BackgroundColor.Rgb;
            if (fillColor == JobFinder.JobCellFillRgb)
            {
                string? currentKey = _ws.Cells[row, col].Value?.ToString();
                
                // Getting first job key in row
                if (jobKeys.Count < 1)
                {
                    if (currentKey != null && JobFinder.SubJobKeys.TryGetValue(currentKey, out JobFinder.SubJobKeyDescription? subDescription))
                    {
                        jobKeys.Add(new JobKeyTracker(subDescription.ParentKey, col, currentKey, col));
                    } else 
                    {
                        jobKeys.Add(new JobKeyTracker(currentKey, col));
                    }
                    firstJobKeyColumn = col;
                }
                // New job key (possible split shift, or possible subjob)
                // Runs when currentKey is a valid job or subjob key that is different from what was last found
                else if (currentKey != null && !JobFinder.NonJobKeys.Contains(currentKey) && currentKey != jobKeys.Last().JobKey && currentKey != jobKeys.Last().SubJobKey)
                {
                    // Subkey handler
                    if (JobFinder.SubJobKeys.TryGetValue(currentKey, out JobFinder.SubJobKeyDescription? subDescription)) 
                    {
                        if (subDescription.ParentKey == jobKeys.Last().JobKey)
                        {
                            jobKeys.Last().SubJobKey = currentKey;
                            jobKeys.Last().SubJobStartColumn = col;
                        } else {
                            jobKeys.Add(new JobKeyTracker(subDescription.ParentKey, col, currentKey, col));
                        }
                    } else 
                    {
                        var previousJobName = JobFinder.JobKeys[jobKeys.Last().JobKey ?? ""];
                        var currentJobName = JobFinder.JobKeys[currentKey];

                        if (
                            !(previousJobName.Contains("Front") && currentJobName.Contains("Front"))
                            && !(!StringHelpers.ContainsOne(previousJobName, ["Front", "Fuel", "Liquor"]) && !StringHelpers.ContainsOne(currentJobName, ["Front", "Fuel", "Liquor"]))
                            && previousJobName != currentJobName
                        )
                        {
                            jobKeys.Add(new JobKeyTracker(currentKey, col));  
                        }
                    }                 
                } else if (jobKeys.Last().JobKey == currentKey && !string.IsNullOrWhiteSpace(jobKeys.Last().SubJobKey))
                {
                    jobKeys.Last().SubJobEndColumn = col;
                }
            }
        }

        // Throwing error if jobStartColumn was never found
        if (firstJobKeyColumn == 0)
        {
            throw new Exception(nameof(firstJobKeyColumn));
        }
        // Throwing error when end is not found, end should always be greater
        if (jobEndColumn <= firstJobKeyColumn)
        {
            throw new Exception(nameof(jobEndColumn));
        }

        DateTime wholeShiftStart = _timeIndex[firstJobKeyColumn];

        JobPosition startingJobPosition;
        if(!string.IsNullOrWhiteSpace(employeePreferences.PositionOverride) && !JobFinder.SubJobKeys.TryGetValue(employeePreferences.PositionOverride, out JobFinder.SubJobKeyDescription? trashValue))
            startingJobPosition = FindJobPosition(employeePreferences.PositionOverride, 1);
        else
            startingJobPosition = FindJobPosition(jobKeys[0].JobKey, row);
        
        // Get split shifts here
        for (int i = 0; i < jobKeys.Count; i++)
        {
            if(!string.IsNullOrWhiteSpace(employeePreferences.PositionOverride) && !JobFinder.SubJobKeys.TryGetValue(employeePreferences.PositionOverride, out JobFinder.SubJobKeyDescription? trashValue2))
            {
                shifts.Add(new ShiftData(wholeShiftStart, wholeShiftEnd, startingJobPosition));
                break;
            }
            else if(!string.IsNullOrWhiteSpace(employeePreferences.PositionOverride) && JobFinder.SubJobKeys.TryGetValue(employeePreferences.PositionOverride, out JobFinder.SubJobKeyDescription? trashValue3))
            {
                shifts.Add(new ShiftData(wholeShiftStart, wholeShiftEnd, startingJobPosition, wholeShiftStart, wholeShiftEnd, trashValue3.Title));
                break;
            }
            if (i == 0)
            {
                var firstShiftEnd = i == jobKeys.Count - 1 ? wholeShiftEnd : _timeIndex[jobKeys[i + 1].JobStartColumn];
                
                if (!string.IsNullOrWhiteSpace(jobKeys[i].SubJobKey))
                {
                    shifts.Add(new ShiftData(wholeShiftStart, firstShiftEnd, startingJobPosition, _timeIndex[jobKeys[i].SubJobStartColumn], jobKeys[i].SubJobEndColumn > -1 ? _timeIndex[jobKeys[i].SubJobEndColumn] : firstShiftEnd, JobFinder.SubJobKeys[jobKeys[i].SubJobKey].Title));
                } else {
                    shifts.Add(new ShiftData(wholeShiftStart, firstShiftEnd, startingJobPosition));
                }
            }
            else if (i == jobKeys.Count - 1)
            {
                if (!string.IsNullOrWhiteSpace(jobKeys[i].SubJobKey))
                {
                    shifts.Add(new ShiftData(_timeIndex[jobKeys[i].JobStartColumn], wholeShiftEnd, FindJobPosition(jobKeys[i].JobKey, row), _timeIndex[jobKeys[i].SubJobStartColumn], jobKeys[i].SubJobEndColumn > -1 ? _timeIndex[jobKeys[i].SubJobEndColumn] : wholeShiftEnd, JobFinder.SubJobKeys[jobKeys[i].SubJobKey].Title));
                } else 
                {
                    shifts.Add(new ShiftData(_timeIndex[jobKeys[i].JobStartColumn], wholeShiftEnd, FindJobPosition(jobKeys[i].JobKey, row)));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(jobKeys[i].SubJobKey))
                {
                    shifts.Add(new ShiftData(_timeIndex[jobKeys[i].JobStartColumn], _timeIndex[jobKeys[i + 1].JobStartColumn], FindJobPosition(jobKeys[i].JobKey, row), _timeIndex[jobKeys[i].SubJobStartColumn], jobKeys[i].SubJobEndColumn > -1 ? _timeIndex[jobKeys[i].SubJobEndColumn] : _timeIndex[jobKeys[i + 1].JobStartColumn], JobFinder.SubJobKeys[jobKeys[i].SubJobKey].Title));
                } else {
                    shifts.Add(new ShiftData(_timeIndex[jobKeys[i].JobStartColumn], _timeIndex[jobKeys[i + 1].JobStartColumn], FindJobPosition(jobKeys[i].JobKey, row)));
                }
            }
        }


        // Prepare shifts for being processed into currentDay

        DateTime? breakOne = null;
        DateTime? lunch = null;
        DateTime? breakTwo = null;
        // Getting breaks for front end employees
        if (shifts.Any(shift => shift.Position.Name.Contains("Front")))
        {
            bool isAdultBetter = !!employeePreferences.Birthday.HasValue;
            bool isAdult = employeePreferences.Birthday.HasValue ? (_currentDay.Date - employeePreferences.Birthday.GetValueOrDefault()).TotalDays >= 6570 : true;
            (breakOne, lunch, breakTwo) = EmployeeHelpers.GetBreaks(
                wholeShiftStart, wholeShiftEnd, employeePreferences.PreferredNumberOfBreaks, !isAdult || employeePreferences.GetsLunchAsAdult); // This is so ugly im so sorry
        }

        foreach (var shift in shifts)
        {
            var shiftBreakOne = breakOne != null && breakOne.Value.TimeOfDay >= shift.Start.TimeOfDay && breakOne.Value.TimeOfDay < shift.End.TimeOfDay ? breakOne : null;
            var shiftLunch = lunch != null && lunch.Value.TimeOfDay >= shift.Start.TimeOfDay && lunch.Value.TimeOfDay < shift.End.TimeOfDay ? lunch : null;
            var shiftBreakTwo = breakTwo != null && breakTwo.Value.TimeOfDay >= shift.Start.TimeOfDay && breakTwo.Value.TimeOfDay < shift.End.TimeOfDay ? breakTwo : null;
            string? jobColumnValue = _ws.Cells[row, _jobColumn].Value?.ToString();
            Subshift? subShift = null;
            if (!string.IsNullOrWhiteSpace(shift.SubJobName)) 
            {
                subShift = new() { ShiftStart = shift.SubStart!.Value, ShiftEnd = shift.SubEnd!.Value, OriginalPosition = !string.IsNullOrWhiteSpace(employeePreferences.OriginalPositionOverride) ? employeePreferences.OriginalPositionOverride : shift.SubJobName };
            }

            // Actual shift processing done here
            CreateAndAddShift(employeePreferences.EmployeeId, !string.IsNullOrWhiteSpace(employeePreferences.PreferredFirstName) ? employeePreferences.PreferredFirstName : employeePreferences.FirstName, !string.IsNullOrWhiteSpace(employeePreferences.PreferredLastName) ? employeePreferences.PreferredLastName : employeePreferences.LastName
                , jobColumnValue ?? "", shift.Start, shift.End, shiftBreakOne, shiftLunch
                , shiftBreakTwo, shift.Position, employeePreferences.BathroomOrder, employeePreferences.IsACallUp, subShift, employeePreferences.OriginalPositionOverride);
        }

    }

    private JobPosition FindJobPosition(string? jobKey, int row)
    {
        string? jobName = string.Empty;
        // File's job key is null as of making this and I could not think of anything other than hardcoding it
        if (string.IsNullOrWhiteSpace(jobKey))
            jobName = "File Clerk";

        else if (jobKey == "F" || jobKey == "P")
        {
            jobName = _ws.Cells[row, _jobColumn].Value?.ToString();
        }
        else if (JobFinder.JobKeys.ContainsKey(jobKey))
        {
            jobName = JobFinder.JobKeys[jobKey];
        }

        if (string.IsNullOrWhiteSpace(jobName))
            jobName = "Misc.";


        if (_currentDay == null)
        {
            throw new NullReferenceException("currentDay was not found and cannot be null");
        }

        var jobPosition = _currentDay.JobPositions.FirstOrDefault(j => j.Name == jobName);
        if (jobPosition == null)
        {
            jobPosition = new JobPosition(jobName);
            if (jobName.Contains("Front") || jobName.Contains("Fuel") || jobName.Contains("Liquor") || jobName.Contains("Pharmacy"))
                _currentDay.JobPositions.Add(jobPosition);
        }


        return jobPosition;
    }


    private void CreateAndAddShift(int id, string firstName, string lastName, string jobColumnValue, DateTime shiftStart,
         DateTime shiftEnd, DateTime? breakOne, DateTime? lunch, DateTime? breakTwo, JobPosition jobPosition, int bathroomOrder, 
         bool isCallUp, Subshift? subShift = null, string? originalPositionOverride = null)
    {

        var newShift = new Shift
        {
            EmployeeId = id.ToString(),
            FirstName = firstName,
            BaggerName = firstName,
            LastName = lastName,
            ShiftStart = shiftStart,
            ShiftEnd = shiftEnd,
            BreakOne = breakOne,
            Lunch = lunch,
            BreakTwo = breakTwo,
            OriginalPosition = !string.IsNullOrWhiteSpace(originalPositionOverride) ? originalPositionOverride : jobColumnValue,
            Subshift = subShift
        };

        if ((!jobPosition.Name.Contains("Front") && !jobPosition.Name.Contains("Liquor") && !jobPosition.Name.Contains("Fuel") && isCallUp)
            || jobPosition.Name.Contains("Floral") || jobPosition.Name.Contains("Apparel") || jobPosition.Name.Contains("File"))
        {

            if (_currentDay == null)
            {
                throw new NullReferenceException("currentDay was not found and cannot be null");
            }
            var callUpPosition = _currentDay.JobPositions.FirstOrDefault(j => j.Name == "Call Ups");
            if (callUpPosition == null)
            {
                callUpPosition = new JobPosition("Call Ups");
                _currentDay.JobPositions.Add(callUpPosition);
            }

            callUpPosition.Shifts.Add(newShift);

        }
        else
        {

            jobPosition.Shifts.Add(newShift);

            // Getting the most appropriate bagger for restrooms
            if ((jobPosition.Name.Contains("Courtesy") || jobPosition.Name.Contains("Utility")) && (jobColumnValue.Contains("Courtesy") || jobColumnValue.Contains("Utility")) && shiftStart.Hour <= 9 && bathroomOrder != 0 && (_bathroomShiftOrder == -1 || bathroomOrder < _bathroomShiftOrder))
            {
                _bathroomShift = newShift;
                _bathroomShiftOrder = bathroomOrder;
            }
        }

    }

    private void MapTimeIndexes(int row)
    {
        bool foundTimes = false;
        bool previousCellWasMerged = false;
        DateTime lastTimeFound = new DateTime(); // Cannot be null due to order of cell checks
        for (int col = 1; col <= _colCount; col++)
        {
            // Checking for 15 minute increments after getting to the time columns which are always null
            // Necessary to check row below as each hour label is merged
            if (foundTimes && _ws.Cells[row, col].Value is null)
            {
                // Columns are sometimes merged causing unneeded stress trying to working around it
                if (_ws.Cells[row + 1, col].Merge)
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
                        _timeIndex.Add(col, lastTimeFound);
                    }
                }
                else
                {
                    previousCellWasMerged = false;
                    lastTimeFound = lastTimeFound.AddMinutes(15);
                    _timeIndex.Add(col, lastTimeFound);
                }
            }

            if (_locationColumn == 0 && _ws.Cells[row, col].Value?.ToString() == "Location")
            {
                _locationColumn = col;
                continue;
            }
            if (_nameColumn == 0 && _ws.Cells[row, col].Value?.ToString() == "Name")
            {
                _nameColumn = col;
                continue;
            }
            if (_jobColumn == 0 && _ws.Cells[row, col].Value?.ToString() == "Job")
            {
                _jobColumn = col;
                continue;
            }
            if (_startColumn == 0 && _ws.Cells[row, col].Value?.ToString() == "Start")
            {
                _startColumn = col;
                continue;
            }
            if (_endColumn == 0 && _ws.Cells[row, col].Value?.ToString() == "End")
            {
                _endColumn = col;
                continue;
            }

            double dateNum;
            if (double.TryParse(_ws.Cells[row, col].Value?.ToString(), out dateNum))
            {
                lastTimeFound = DateTime.FromOADate(dateNum);
                _timeIndex.Add(col, lastTimeFound);
                previousCellWasMerged = _ws.Cells[row + 1, col].Merge;
            }
            if (!foundTimes && _timeIndex.Count > 0) foundTimes = true;
        }
    }

    private void SortAllShifts()
    {
        foreach (var day in _days)
            foreach (var jobPosition in day.JobPositions)
                jobPosition.Shifts.Sort((x, y) => x.ShiftStart.CompareTo(y.ShiftStart));
    }

}
