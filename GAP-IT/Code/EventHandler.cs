using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace GAP_IT.Code
{
    class EventHandler
    {
        // Basic properties
        public int[] EventID { get; set; }
        public string EventSource { get; set; }
        public int Days { get; set; }
        public int Limit { get; set; }
        public string[,] Collect(string eventLogFile = null)
        {
            // Variable for the results
            var allResults = new string[0, 0];
            var eventResults = new List<DateTime>();
            var eventUsers = new List<string>();
            var eventMessage = new List<string>();
            var eventId = new List<int>();
            
            // New eventSession
            var eventSession = new EventLogSession();

            // Try collect the events
            try
            {
                if (ProgramSettings.Remote)
                {
                    eventSession = new EventLogSession(ProgramSettings.Machine, ProgramSettings.Domain, ProgramSettings.Username, ProgramSettings.Password, SessionAuthentication.Default);
                }
                else
                {
                    eventSession = new EventLogSession(Environment.MachineName);
                }

                var eventLogQuery = new EventLogQuery(EventSource, PathType.LogName)
                {
                    ReverseDirection = true,
                    Session = eventSession
                };

                if (eventLogFile != null)
                {
                    eventLogQuery = new EventLogQuery(eventLogFile, PathType.FilePath)
                    {
                        ReverseDirection = true,
                        Session = eventSession
                    };

                }

                var eventReader = new EventLogReader(eventLogQuery);
                
                var eventLog = eventReader.ReadEvent();
                var useTimeHistory = false;

                var maxTimeCreate = DateTime.Now;
                
                // Remove amount of days
                if (Days != 0)
                {
                    maxTimeCreate = maxTimeCreate.AddDays(-Days);
                    useTimeHistory = true;
                }

                // Reading eventlog and filling lists
                while (eventLog != null)
                {
                    if (useTimeHistory)
                    {
                        if (eventLog.TimeCreated >= maxTimeCreate)
                        {
                            foreach (var iD in EventID)
                            {
                                if (eventLog.Id == iD)
                                {
                                    eventResults.Add(eventLog.TimeCreated.Value);
                                    eventUsers.Add(eventLog.UserId.ToString());
                                    eventMessage.Add(eventLog.FormatDescription());
                                    eventId.Add(iD);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    eventLog = eventReader.ReadEvent();

                    // Use the limit if configured
                    if (Limit != 0)
                    {
                        if (eventResults.Count >= Limit)
                            break;
                    }
                }

                // Close session to reduce open connections and inuse memory
                eventSession.Dispose();
                eventReader.Dispose();
            }
            catch (Exception)
            {
                // Catch
            }

            // Creating complete array
            if (eventResults.Count != 0)
            {

                allResults = new string[eventResults.Count, 4];
                var i = 0;

                do
                {
                    // Set datetime in specific format
                    allResults[i, 0] = eventResults[i].ToString("MM/dd/yyyy H:mm:ss:fff");
                    // Set userName
                    allResults[i, 1] = eventUsers[i];
                    // Set the EventText
                    allResults[i, 2] = eventMessage[i];
                    // Set eventID
                    allResults[i, 3] = eventId[i].ToString();

                    i = i + 1;
                } while (i <= eventResults.Count - 1);
            }
            
            // Dispose open session
            eventSession.Dispose();

            // Return results
            return allResults;
        }
    }
}
