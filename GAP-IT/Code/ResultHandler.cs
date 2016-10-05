using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GAP_IT.Models;

namespace GAP_IT.Code
{
    class ResultHandler
    {
        public string[,] Logon { get; set; }
        public string[,] AppV { get; set; }

        public List<EventEntry> BuildSet()
        {
			// resgity handler to resolve package information
            RegistryHandler reg = new RegistryHandler();

            // Set DateTime format
            var parseFormat = "MM/dd/yyyy H:mm:ss:fff";
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            // create used variables
            var package = new PackageHandler();

            DateTime time;
            var id = 0;
            var eventId = "";
            var packageId = "";
            var versionId = "";
            var packageName = "";
            var packageVersion = "";
            var userSid = "";
			
            // loop trough event results
            var i = 0;
            var appvEventList = new List<EventEntry>();
            do
            {
				var eventEntry = new EventEntry();

                // set time
                time = DateTime.ParseExact(AppV[i, 0], parseFormat, cultureInfo);

                // set eventId
                eventId = AppV[i, 3];

                // set userSID
                userSid = AppV[i, 1];

                // set package information
                if (eventId == "19001" || eventId == "19002")
                {
                    // Set packageId & versionId to 0
                    packageId = "00000000-0000-0000-0000-000000000000";
                    versionId = "00000000-0000-0000-0000-000000000000";

                    // Set packagename and version to refresh
                    packageName = "Refresh";
                    packageVersion = "0.0.0";
                }
                else
                {
                    // collect the packageId & versionId
                    package.AppVEvents = AppV[i, 2];
                    packageId = package.PackageID();
                    versionId = package.VersionID();
                }
				
                // resolve packageName & version
                var packageInfo = new string[1, 2];
                var completePackageName = reg.LoadPackage(packageId, versionId);
                if (completePackageName == "")
                {
                    packageInfo = package.ResolveName(packageId, versionId);
                    reg.SavePackage(packageId, versionId, packageInfo[0, 0], packageInfo[0, 1]);
                }
                else
                {
                    var tempPackageName = completePackageName.Split('/');
                    packageInfo[0, 0] = tempPackageName[0];
                    packageInfo[0, 1] = tempPackageName[1];
                }

				// define packagename and package version
                packageName = packageInfo[0, 0];
                packageVersion = packageInfo[0, 1];

                eventEntry.EventId = eventId;
                eventEntry.Time = time;
                eventEntry.EventSet = id;
				eventEntry.User = new User
				{
				    UserSid = userSid
				};
                eventEntry.Package = new Package
                {
					Guid = packageId,
					Name = packageName,
					VersionGuid = versionId,
					Version = packageVersion
                };

                appvEventList.Add(eventEntry);

                // increase the id to new set
                if (eventId == "19001")
                {
                    eventEntry = new EventEntry();
                    id++;
                }

                i++;
            } while (i <= AppV.GetLength(0) - 1);
            return appvEventList;
        }

        public List<Timings> Parse(List<EventEntry> appvEventList)
        {

            // define return
            var returnResults = new List<Timings>();

            // collect all App-V based events by event set
            var appvEventsListByEventSets = appvEventList.GroupBy(a => a.EventSet).ToList();


            foreach (var appvEventsListByEventSet in appvEventsListByEventSets)
            {
                // exclude the mounts for now (trying to restore existing functionnality)
                var publishingTimesEvents = appvEventsListByEventSet.Where(b => b.EventId == "19001" || b.EventId == "19002"  || b.EventId == "1003").ToArray();

                // check if list contains results which can be a min of 2
                if (publishingTimesEvents.Count() >= 2)
                {
                    // define i variable for loop
                    var i = 0;

                    // loop through events for timing calulations
                    do
                    {
                        // check to avoid out of index
                        if (i != publishingTimesEvents.Count() - 1)
                        {
                            // create result entry
                            var publishingResult = new Timings();

                            // create timing variables
                            var startPublishingTime = publishingTimesEvents[i + 1].Time;
                            var endPublishingTime = publishingTimesEvents[i].Time;

                            // defining 

                            // adding used events
                            publishingResult.EventEntry1 = publishingTimesEvents[i + 1];
                            publishingResult.EventEntry2 = publishingTimesEvents[i];
                            publishingResult.Timing = Convert.ToInt32(endPublishingTime.Subtract(startPublishingTime).TotalMilliseconds);

                            // add result to return list
                            returnResults.Add(publishingResult);
                            
                            // increment i
                            i++;
                        }
                    } while (i != publishingTimesEvents.Count() -1);
                }
            }
            // return results
            return returnResults;
        }

        public void Export(string file, List<Timings> timingList)
        {

            var dataSet = timingList.ToArray();

            StreamWriter fileWriter = new StreamWriter(file, true);

            // Header
            // Result format: StartTime, ResultTime, UserSID, PackgeID, VersionID, PackageName, Version, ResultSet
            string resultHeader = "Start time, Result ID, User SID, Package ID, Version ID, Package name, Version, Publishing time";

            fileWriter.WriteLine(resultHeader);

            var i = 0;

            do
            {
                var startTime = dataSet[i].EventEntry1.Time;
                var resultTime = dataSet[i].Timing;
                var userSid = dataSet[i].EventEntry1.User.UserSid;
                var packageId = dataSet[i].EventEntry1.Package.Guid;
                var versionId = dataSet[i].EventEntry1.Package.VersionGuid;
                var packageName = dataSet[i].EventEntry1.Package.Name;
                var packageVersion = dataSet[i].EventEntry1.Package.Version;
                var resultSet = dataSet[i].EventEntry1.EventSet;

                string result = startTime + ", " + resultSet + ", " + userSid + ", " + packageId + ", " + versionId + ", " + packageName + ", " + packageVersion + ", " + resultTime;
                fileWriter.WriteLine(result);
                i++;

            } while (dataSet.Count() - 1 >= i);

            fileWriter.Close();
        }
    }

}

//            //var = eventEntry.Timings.Where(t => t.Timing == 40);
//            // var i = 0;
//            var appvEventsListByEventSet = appvEventList.GroupBy(a => a.EventSet).ToList();
//
//            var startPublishingTime = new DateTime();
//            var endPublishingTime = new DateTime();
//
//            foreach (var appvEventPerEvent in appvEventsListByEventSet)
//            {
//
//                var publishingTimeTotalEvents = appvEventPerEvent.Where(b => b.EventId == "19001" || b.EventId == "19002").ToArray();
//
//                if (publishingTimeTotalEvents.Count() >= 2)
//                {
//                    var publishingTimeTotalResult = new Timings();
//                    publishingTimeTotalResult.Type = "Total publishing time";
//
//                    foreach (var publishingTimeTotalEvent in publishingTimeTotalEvents)
//                    {
//                        if (publishingTimeTotalEvent.EventId == "19001")
//                        {
//                            startPublishingTime = publishingTimeTotalEvent.Time;
//                            publishingTimeTotalResult.EventEntry1 = publishingTimeTotalEvent;
//                        }
//
//                        if (publishingTimeTotalEvent.EventId == "19002")
//                        {
//                            endPublishingTime = publishingTimeTotalEvent.Time;
//                            publishingTimeTotalResult.EventEntry2 = publishingTimeTotalEvent;
//                        }
//                    }
//
//                    publishingTimeTotalResult.Timing = endPublishingTime.Subtract(startPublishingTime).TotalMilliseconds;
//                }
//
//                var publishingTimePerPackageEvents = appvEventPerEvent.Where(b => b.EventId == "1003").ToArray();
//
//                if (publishingTimePerPackageEvents.Count() >= 1)
//                {
//                    foreach (var publishingTimePerPackageEvent in publishingTimePerPackageEvents)
//                    {
//                        var packageMountEvents = appvEventPerEvent.Where(b => b.EventId == "1002" && b.Package.Guid == publishingTimePerPackageEvent.Package.Guid && b.Package.VersionGuid == publishingTimePerPackageEvent.Package.VersionGuid).ToArray();
//                        if (packageMountEvents.Count() >= 1)
//                        {
//                            // do stuff
//
//                            var publishTimeResult = new Timings();
//                            publishTimeResult.Type = "Package publishing time";
//
//                            foreach (var packageMountEvent in packageMountEvents)
//                            {
//                                if (packageMountEvent.EventId == "1002")
//                                {
//                                    startPublishingTime = packageMountEvent.Time;
//                                    publishTimeResult.EventEntry1 = packageMountEvent;
//                                }
//                            }
//
//                            endPublishingTime = publishingTimePerPackageEvent.Time;
//                            publishTimeResult.EventEntry2 = publishingTimePerPackageEvent;
//                            publishTimeResult.Timing = endPublishingTime.Subtract(startPublishingTime).TotalMilliseconds;
//
//                        }
//                        else
//                        {
//                            var packagestartyolosomethingdontknow = appvEventPerEvent.Where(b => b.Time <= publishingTimePerPackageEvent.Time).Where(c => c.EventId == "1003" || c.EventId == "19001").Where(d => d.Package.Guid != publishingTimePerPackageEvent.Package.Guid && d.Package.VersionGuid != publishingTimePerPackageEvent.Package.VersionGuid).ToArray();
//
//                            if (packagestartyolosomethingdontknow.Count() >= 1)
//                            {
//                                // do stuff
//                            }
//                        }
//                    }
//                }

//            }

