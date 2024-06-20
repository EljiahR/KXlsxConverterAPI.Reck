namespace KXlsxConverterAPI.Models.ScheduleModels
{
    public class Shift
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
        public DateTime? BreakOne { get; set; } = null;
        public DateTime? BreakTwo { get; set; } = null;
        public DateTime? Lunch { get; set; } = null;
    }
}
