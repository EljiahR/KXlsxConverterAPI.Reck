namespace KXlsxConverterAPI.Helpers
{
    public class JobFinder
    {
        // Dictionary used to properly group employees together
        // These are supject to change randomly for no reason, pretty much whenever
        public Dictionary<char, string> jobKeys = new()
        {
            // Front End Keys, given breaks
            {'P', "Front End Supervisor" },
            {'U', "Front End SCO Cashier" },
            {'$', "Front End Cashier" },
            {'B', "Front End Courtesy Clerk" },
            {'!', "Front End Service Desk" },
            
            // Common jobs for front end employees to have a split shift with
            {'Z', "Fuel Clerk" },
            {'L', "Liquor Clerk" },

            // Job keys that are typically used as call ups for front end
            {'C', "HBC Clerk" },
            {'M', "GM MrktPlc Clk" }, // Not sure why so heavily shortened
            {'8', "File Clerk" } // Not the actual key, which is null, just don't want to leave it out yet
        };
    }
}
