﻿using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;
using KXlsxConverterAPI.Models.ScheduleModels.Interfaces;
using OfficeOpenXml;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KXlsxConverterAPI.Helpers;

public class XlsxConverter
{
    private enum _CurrentSection { FoundNewDay, FoundEmployee, FoundNothing };
    private static Regex _dayOfWeekRegex = new Regex(@"^[A-Z]{1}[a-z]{2}\s\d{2}\/\d{2}\/\d{4}$");
    private static Regex _nameRegex = new Regex(@"^[a-zA-Z]+,\s[a-zA-Z]+");

    private List<WeekdaySchedule> _days; // New list of days to be built as rows are checked

    //Column numbers are occasionally different due to random extra merged columns, so these next few variables are for fixing that
    private Dictionary<int, DateTime> _timeIndex; // Index for what time each column represents
    private int _nameColumn = 0;
    // Last three columns may not be necessary depending on how reliable the jobkey is with the time columns
    private int _locationColumn = 0;
    private int _jobColumn = 0;
    private int _startColumn = 0;
    private int _endColumn = 0;

    private WeekdaySchedule? _currentDay;
    private ExcelWorksheet _ws;
    private int _rowCount = 0;
    private int _colCount = 0;

    private Shift? _bathroomShift = null;
    private int _bathroomShiftOrder = -1;

    private IEnumerable<Employee> _storeEmployees;

    public XlsxConverter(IEnumerable<Employee> storeEmployees, ExcelWorksheet ws)
    {
        _days = new();
        _timeIndex = new();
        _storeEmployees = storeEmployees; // Injecting whatever employee list was found before this converter was created
        _ws = ws;
    }
    public List<WeekdaySchedule> ConvertXlsx()
    {
        _rowCount = _ws.Dimension.Rows;
        _colCount = _ws.Dimension.Columns;

        _currentDay = null;
        bool daysFound = false;
        for (int row = 1; row <= _rowCount; row++)
            switch (IdentifyRow(row, daysFound))
            {
                case _CurrentSection.FoundNewDay:
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
                        if (baggerShifts != null) EmployeeHelpers.FillCarts(_currentDay.Carts, baggerShifts, _bathroomShift);
                    }

                    _currentDay = new WeekdaySchedule(newWeekday.ToString("dddd"), newWeekday);
                    // Reseting bathroom bagger
                    _bathroomShift = null;
                    _bathroomShiftOrder = -1;
                    break;
                case _CurrentSection.FoundEmployee:
                    try
                    {
                        ParseEmployeeRow(row);
                    }
                    catch (Exception ex) 
                    {
                        if(_currentDay != null)
                        {
                            string errorType = ex.GetType().Name;
                            if (_currentDay.Errors.ContainsKey(errorType))
                                _currentDay.Errors[errorType].Add("Error in row " + row);
                            else
                                _currentDay.Errors.Add(errorType, new List<string> { "Error in row " + row });
                        }
                            
                    }
                    break;

            }


        // Filling carts in for the last day
        if (_currentDay != null)
        {
            var baggerShifts = _currentDay.JobPositions.Where(x => x.Name == "Front End Courtesy Clerk").FirstOrDefault();
            if (baggerShifts != null) EmployeeHelpers.FillCarts(_currentDay.Carts, baggerShifts, _bathroomShift);
            _days.Add(_currentDay);
        }
        SortAllShifts();
        return _days;
    }

    private _CurrentSection IdentifyRow(int row, bool daysFound)
    {
        if (_dayOfWeekRegex.IsMatch(_ws.Cells[row, 1].Value?.ToString() ?? ""))
            return _CurrentSection.FoundNewDay;
        else if (daysFound && _nameRegex.IsMatch(_ws.Cells[row, _nameColumn].Value?.ToString() ?? ""))
            return _CurrentSection.FoundEmployee;
        return _CurrentSection.FoundNothing;
    }

    private void ParseEmployeeRow(int row)
    {
        if (_currentDay == null) throw new ArgumentNullException("currentDay should not be null");

        var shifts = new List<(DateTime start, DateTime end, JobPosition jobPosition)>();
        string firstName, lastName;
        var nameCellValue = _ws.Cells[row, _nameColumn].Value?.ToString();

        if (string.IsNullOrEmpty(nameCellValue)) throw new ArgumentNullException($"Name cell at row:{row} column:{_nameColumn} was null");

        (firstName, lastName) = StringFixer.GetFirstAndLastName(nameCellValue);
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

        string? fillColor;

        // Finding end of shift

        int jobEndColumn = 0;
        for (int col = _colCount; col > 1; col--)
        {
            // Actual time ending would be the cell right after the last filled cell, hence col - 1 for all checks
            fillColor = _ws.Cells[row, col - 1].Style.Fill.BackgroundColor.Rgb;
            if (fillColor == JobFinder.jobCellFillRgb)
            {
                jobEndColumn = col;
                break;
            }
        }

        // Fixing any problems caused by nefarious merged time cells
        if (!_timeIndex.ContainsKey(jobEndColumn))
        {
            jobEndColumn--;
            if (!_timeIndex.ContainsKey(jobEndColumn)) throw new ArgumentOutOfRangeException(nameof(jobEndColumn));
        }
        DateTime wholeShiftEnd = _timeIndex[jobEndColumn];

        List<(string? jobKey, int jobStartColumn)> jobKeys = new();
        int firstJobKeyColumn = 0;

        // Finding beginning of shift and all splits 

        for (int col = 1; col <= jobEndColumn; col++)
        {
            fillColor = _ws.Cells[row, col].Style.Fill.BackgroundColor.Rgb;
            if (fillColor == JobFinder.jobCellFillRgb)
            {
                string? currentKey = _ws.Cells[row, col].Value?.ToString();
                if (jobKeys.Count < 1)
                {
                    jobKeys.Add((currentKey, col));
                    firstJobKeyColumn = col;
                }
                else if(currentKey != null && !JobFinder.NonJobKeys.Contains(currentKey) && currentKey != jobKeys.Last().jobKey)
                {
                    
                    var previousJobName = JobFinder.jobKeys[jobKeys.Last().jobKey ?? ""];
                    var currentJobName = JobFinder.jobKeys[currentKey];

                    if(!(previousJobName.Contains("Front") && currentJobName.Contains("Front")) && previousJobName != currentJobName)
                        jobKeys.Add((currentKey, col));
                }
                    
            }
        }
        // Throwing error if jobStartColumn was never found
        if (firstJobKeyColumn == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(firstJobKeyColumn));
        }
        // Throwing error when end is not found, end should always be greater
        if (jobEndColumn <= firstJobKeyColumn)
        {
            throw new ArgumentOutOfRangeException(nameof(jobEndColumn));
        }

        DateTime wholeShiftStart = _timeIndex[firstJobKeyColumn];

        var startingJobPosition = FindJobPosition(jobKeys[0].jobKey, row);
        // Get split shifts here
        for (int i = 0; i < jobKeys.Count; i++)
        {
            if(i == 0)
            {
                var firstShiftEnd = i == jobKeys.Count - 1 ? wholeShiftEnd : _timeIndex[jobKeys[i + 1].jobStartColumn];
                shifts.Add((wholeShiftStart, firstShiftEnd, startingJobPosition));
            } else if(i == jobKeys.Count - 1)
            {
                shifts.Add((_timeIndex[jobKeys[i].jobStartColumn], wholeShiftEnd, FindJobPosition(jobKeys[i].jobKey, row)));
            } else
            {
                shifts.Add((_timeIndex[jobKeys[i].jobStartColumn], _timeIndex[jobKeys[i + 1].jobStartColumn], FindJobPosition(jobKeys[i].jobKey, row)));
            }
        }


        // Prepare shifts for being processed into currentDay
        
        DateTime? breakOne = null;
        DateTime? lunch = null;
        DateTime? breakTwo = null;
        // Getting breaks for front end employees
        if (shifts.Any(shift => shift.jobPosition.Name.Contains("Front")))
        {
            bool isAdult = employeePreferences.Birthday.HasValue ? (_currentDay.Date - employeePreferences.Birthday.GetValueOrDefault()).TotalDays >= 6570 : true;
            (breakOne, lunch, breakTwo) = EmployeeHelpers.GetBreaks(
                wholeShiftStart, wholeShiftEnd, employeePreferences.PreferredNumberOfBreaks, !isAdult || employeePreferences.GetsLunchAsAdult); // This is so ugly im so sorry
        }

        foreach (var shift in shifts)
        {
            var shiftBreakOne = breakOne != null && breakOne.Value.TimeOfDay >= shift.start.TimeOfDay && breakOne.Value.TimeOfDay < shift.end.TimeOfDay ? breakOne : null;
            var shiftLunch = lunch != null && lunch.Value.TimeOfDay >= shift.start.TimeOfDay && lunch.Value.TimeOfDay < shift.end.TimeOfDay ? lunch : null;
            var shiftBreakTwo = breakTwo != null && breakTwo.Value.TimeOfDay >= shift.start.TimeOfDay && breakTwo.Value.TimeOfDay < shift.end.TimeOfDay ? breakTwo : null;
            string? jobColumnValue = _ws.Cells[row, _jobColumn].Value?.ToString();

            // Actual shift processing done here
            CreateAndAddShift(employeePreferences.PreferredFirstName ?? employeePreferences.FirstName, employeePreferences.LastName
                , jobColumnValue ?? "", shift.start, shift.end, shiftBreakOne, shiftLunch
                , shiftBreakTwo, shift.jobPosition, employeePreferences.BathroomOrder, employeePreferences.IsACallUp);
        }

    }

    private JobPosition FindJobPosition(string? jobKey, int row)
    {
        string? jobName = string.Empty;
        // File's job key is null as of making this and I could not think of anything other than hardcoding it
        if (string.IsNullOrEmpty(jobKey))
            jobName = "File Clerk";
        
        else if (jobKey == "F")
            jobName = _ws.Cells[row, _jobColumn].Value?.ToString();
        
        else if (JobFinder.jobKeys.ContainsKey(jobKey))
            jobName = JobFinder.jobKeys[jobKey];
        
        if(string.IsNullOrEmpty(jobName))
            jobName = "Miscellaneous";
        

        if (_currentDay == null){
            throw new NullReferenceException("currentDay was not found and cannot be null"); 
        }

        var jobPosition = _currentDay.JobPositions.Where(j => j.Name == jobName).FirstOrDefault();
        if (jobPosition == null)
        {
            jobPosition = new JobPosition(jobName);
            if(jobName.Contains("Front") || jobName.Contains("Fuel") || jobName.Contains("Liquor"))
                _currentDay.JobPositions.Add(jobPosition);
        }
            

        return jobPosition;
    }


    private void CreateAndAddShift(string firstName, string lastName,string jobColumnValue, DateTime shiftStart
        , DateTime shiftEnd, DateTime? breakOne, DateTime? lunch, DateTime? breakTwo, JobPosition jobPosition, int bathroomOrder, bool isCallUp)
    {

        if (!jobPosition.Name.Contains("Front") && !jobPosition.Name.Contains("Liquor") && !jobPosition.Name.Contains("Fuel"))
        {
            if(isCallUp)
            {
                CallUpShift newCallUpShift = new CallUpShift();

                newCallUpShift.FirstName = firstName;
                newCallUpShift.BaggerName = firstName;
                newCallUpShift.LastName = lastName;
                newCallUpShift.ShiftStart = shiftStart;
                newCallUpShift.ShiftEnd = shiftEnd;
                newCallUpShift.BreakOne = breakOne;
                newCallUpShift.Lunch = lunch;
                newCallUpShift.BreakTwo = breakTwo;
                newCallUpShift.OriginalPosition = jobPosition.Name;

                if (_currentDay == null)
                {
                    throw new NullReferenceException("currentDay was not found and cannot be null");
                }
                var callUpPosition = _currentDay.JobPositions.Where(j => j.Name == "Call Ups").FirstOrDefault();
                if (callUpPosition == null)
                {
                    callUpPosition = new JobPosition("Call Ups");
                    _currentDay.JobPositions.Add(callUpPosition);
                }
                    
                callUpPosition.Shifts.Add(newCallUpShift);

            }
            
        } else
        {
            var newShift = new Shift();

            newShift.FirstName = firstName;
            newShift.BaggerName = firstName;
            newShift.LastName = lastName;
            newShift.ShiftStart = shiftStart;
            newShift.ShiftEnd = shiftEnd;
            newShift.BreakOne = breakOne;
            newShift.Lunch = lunch;
            newShift.BreakTwo = breakTwo;


            jobPosition.Shifts.Add(newShift);

            // Getting the most appropriate bagger for restrooms
            if (jobPosition.Name.Contains("Courtesy") && jobColumnValue.Contains("Courtesy") && shiftStart.Hour <= 7 && bathroomOrder != 0 && (_bathroomShiftOrder == -1 || bathroomOrder < _bathroomShiftOrder))
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
                jobPosition.Shifts.Sort((x,y) => x.ShiftStart.CompareTo(y.ShiftStart));
    }

}
