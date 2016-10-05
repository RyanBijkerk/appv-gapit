namespace GAP_IT.Code
{
    class EventAppV
    {
        public string[,] Collect()
        {
            // Create array for the EventIDs
            int[] eventIds = { 19001, 19002, 1002, 1003 };

            // Create event session with settings
            var eventSession = new EventHandler()
            {
                EventID = eventIds,
                EventSource = "Microsoft-AppV-Client/Operational",
                Days = ProgramSettings.Days
            };

            // Collect the events
            var results = eventSession.Collect(@"C:\Users\Ryan\Downloads\AppVOp.evtx");

            // Return results
            return results;
        }
    }
}
