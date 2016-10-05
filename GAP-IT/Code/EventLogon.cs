namespace GAP_IT.Code
{
    public class LogonEvents
    {
        public string[,] Collect()
        {
            // Create array for the EventIDs
            int[] eventIds = { 2 };

            // Create event session with settings
            var eventSession = new EventHandler()
            {
                EventID = eventIds,
                EventSource = "Microsoft-Windows-User Profile Service/Operational",
                Days = ProgramSettings.Days
            };

            // Collect the events
            var results = eventSession.Collect();

            // Return results
            return results;
        }
    }
}

