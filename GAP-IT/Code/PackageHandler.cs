using System;
using System.Net;
using System.Windows;
using System.Xml;

namespace GAP_IT.Code
{
    class PackageHandler
    {
        public string AppVEvents { get; set; }
        public string[,] PackageIDs { get; set; }

        public string PackageID()
        {
            // Default splits .Split('{', '}')[1];

            var packageId = "";

            // Check if the Event Message contains the App-V package format
            if (AppVEvents.Contains("{") & AppVEvents.Contains("-") & AppVEvents.Contains("}"))
            {
                try
                {
                    // Split message for PackageID
                    packageId = (AppVEvents.Split('{', '}')[1]);
                }
                catch (Exception)
                {   
                    // Set package id to failed
                    packageId = "01010101-0000-0000-0000-000000000000";
                }
            }
            return packageId;
        }

        public string VersionID()
        {
            // Default splits .Split('{', '}')[3];
            var versionId = "";

            // Check if the Event Message contains the App-V package format
            if (AppVEvents.Contains("{") & AppVEvents.Contains("-") & AppVEvents.Contains("}"))
            {
                try
                {
                    // Split message for VersionID
                    versionId = (AppVEvents.Split('{', '}')[3]);
                }
                catch (Exception)
                {
                    // set version id to failed
                    versionId = "01010101-0000-0000-0000-000000000000";
                }            
            }
            return versionId;
        }

        public string[,] ResolveName(string packageId, string versionId)
        {
            var packageName = "Unknown";
            var packageVersion = "0.0.0";


            if (packageId == "00000000-0000-0000-0000-000000000000" &&
                versionId == "00000000-0000-0000-0000-000000000000")
            {
                packageName = "Refresh";
                packageVersion = "0.0.0";

            }
            else
            {
                if (ProgramSettings.Resolve)
                {
                    WebClient client = new WebClient();
                    client.UseDefaultCredentials = true;
                    string appvServer = ProgramSettings.Protocol + ProgramSettings.Address + ":" + ProgramSettings.Port;

                    // Build API url
                    var url = appvServer + "/packages/ByGuid?package={" + packageId + "}&version={" + versionId + "}";

                    try
                    {
                        CredentialCache credentailCache = new CredentialCache();
                        NetworkCredential networkCredential = new NetworkCredential();

                        networkCredential.UserName = ProgramSettings.Username;
                        networkCredential.SecurePassword = ProgramSettings.Password;
                        networkCredential.Domain = ProgramSettings.Domain;

                        credentailCache.Add(new Uri(url), "Digest", networkCredential);

                        //client.Credentials = credentailCache;

                        string response = client.DownloadString(url);

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(response);

                        string xpath = "ArrayOfPackageVersion/PackageVersion";
                        var nodes = xmlDoc.SelectNodes(xpath);

                        foreach (XmlNode childrenNode in nodes)
                        {
                            packageName = childrenNode["Name"].InnerText;
                            packageVersion = childrenNode["Version"].InnerText;
                        }

                        client.Dispose();
                    }
                    catch (Exception errorException)
                    {
                        MessageBox.Show("Cannot get the package name \n URL: " + url + " \n\nExepction:\n\n" + errorException + "");
                    }
                }
            }

            // Build return array
            string[,] packageInfo = new string[1, 2];

            packageInfo[0, 0] = packageName;
            packageInfo[0, 1] = packageVersion;

            // Return
            return packageInfo;
        }
    }
}

