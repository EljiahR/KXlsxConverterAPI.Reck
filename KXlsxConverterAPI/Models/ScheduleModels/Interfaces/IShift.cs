namespace KXlsxConverterAPI.Models.ScheduleModels.Interfaces
{
    public interface IShift
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string BaggerName { get; set; }
        DateTime ShiftStart { get; set; }
        DateTime ShiftEnd { get; set; }
        DateTime? BreakOne { get; set; }
        DateTime? Lunch { get; set; }
        DateTime? BreakTwo { get; set; }
    }
}
