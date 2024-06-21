using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Models.ScheduleModels;

namespace KXlsxConverterAPI.Helpers
{
    public class XlsxConverter
    {
        public static List<WeekdaySchedule> ConvertXlsx(IFormFile file, IEnumerable<Employee> storeEmployees)
        {
            List<WeekdaySchedule> days = new List<WeekdaySchedule>();
            days.Add(new WeekdaySchedule("Sunday", DateTime.Today));

            return days;
        }
    }
}
