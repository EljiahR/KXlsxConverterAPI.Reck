namespace KXlsxConverterAPI.Helpers
{
    public class JobFinder
    {
        // These are supject to change randomly for no reason, pretty much whenever
        public Dictionary<char, string> jobKeys = new()
        {
            {'$', "Front End Cashier" },
            {'B', "Front End Courtesy Clerk" },
            {'U', "Front End SCO Cashier" },
            {'!', "Front End Service Desk" },
            {'P', "Front End Supervisor" }
        };
    }
}
