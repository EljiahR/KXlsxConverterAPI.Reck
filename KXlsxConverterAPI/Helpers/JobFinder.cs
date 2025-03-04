namespace KXlsxConverterAPI.Helpers
{
    public class JobFinder
    {
        // Dictionary used to properly group employees together
        // These are supject to change randomly for no reason, pretty much whenever
        public static readonly Dictionary<string, string> JobKeys = new()
        {
            // Front End Keys, given breaks
            {"P", "Front End Supervisor" },
            {"U", "Front End SCO Cashier" },
            {"$", "Front End Cashier" },
            {"B", "Front End Courtesy Clerk" },  
            {"!", "Front End Service Desk" },
            
            // Common jobs for front end employees to have a split shift with
            {"Z", "Fuel Clerk" },
            {"L", "Liquor Clerk" },
            {"W", "Liquor Clerk" }, // Actually Wine/Beer Steward but thats not necessary to know
            
            // Job keys that are typically used as call ups for front end
            {"C", "HBC Clerk" },
            {"M", "GM MrktPlc Clk" }, // Not sure why so heavily shortened
            {"K", "GM MrktPlc Clk" }, // Kitchen Place?
            {"", "File Clerk" }, // Actual key is null
            {"F", "Floral Clerk" },
            {"A", "Apparel Clerk" } // Has recently been F for some reason, overlapping with Floral
        };

        // For tracking sub jobs that are still in the parent section but needed for other purposes
        // Structured as SubJob's key ->
        // i.e. Utility clerks should not be scheduled for carts
        public static readonly Dictionary<string, SubJobKeyDescription> SubJobKeys = new()
        {
            {"/", new SubJobKeyDescription("B", "Front End Utility Clerk")}, 
        };

        public class SubJobKeyDescription(string parentKey, string title)
        {
            public string ParentKey = parentKey;
            public string Title = title;

        }

        // Random characters that need to be ignored in any given employee row to help find real split shifts
        // The ~ and ^ are breaks and lunches I think. The rest are trash icons typically from bad reports which can sometimes be fixed by re-running the report. 
        public static readonly string[] NonJobKeys = new string[] { "~", "^", "•", "*", "=", "-" };

        public static readonly string JobCellFillRgb = "FFC0C0C0"; // Tried the index and that didn't work sooo Rgb

    }
}
