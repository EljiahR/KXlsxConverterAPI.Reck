namespace KXlsxConverterAPI.Models.ScheduleModels
{
    public class CartSlot
    {
        public DateTime Time { get; set; }
        public string[] Baggers { get; set; } = new string[4]; 
        public CartSlot(DateTime time) 
        { 
            Time = time;
        }
    }
}
