﻿namespace KXlsxConverterAPI.Models;

public class Employee
{
    public int EmployeeId { get; set; } // Not based on any actual corporate based IDs
    public string FirstName { get; set; } = string.Empty; // Name registered in MyTime 
    public string? PreferredFirstName { get; set; } // Name employee prefers if there is one
    public string LastName { get; set; } = string.Empty; // Don't make me explain it
    public DateTime? Birthday { get; set; } // Only Year, Month, and Day necessary
    public bool HideBirthday {get; set;} = false;
    public int PreferredNumberOfBreaks { get; set; } = 2; // Preferred number of break if working >= 6 hours, should default to 2

    // Minors get a mandatory 30 minute unpaid lunch when working >= 6 hours, deducts from shift length
    public bool GetsLunchAsAdult { get; set; } = false;

    // Should be null more often than not and used sparingly, mostly for fixes small bugs/errors caused by the schedule, can use DELETE 
    public string? PositionOverride { get; set; }

    // Should default to 0 unless a bagger and trained to clean bathrooms in the morning, should go in order of 1 scheduled 1st, 2 2nd, etc...
    public int BathroomOrder { get; set; } = 0;

    // For employees either not in front-end and can be called up or front-end employees that are occasionally scheduled in different departments
    public bool IsACallUp { get; set; } = false;
    public int Division { get; set; } = 0;
    public int StoreNumber { get; set; } = 0;
}
