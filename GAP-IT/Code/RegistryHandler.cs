using System;
using Microsoft.Win32;

namespace GAP_IT.Code
{
    class RegistryHandler
    {
        private string regLocation = "Software\\Logitblog\\GAP-IT";

        public bool CreatePackageLocation()
        {
            var packageRegLocation = regLocation + "\\Packages";
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(packageRegLocation);
                key.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool CreateDefaultSettings()
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(regLocation);

                // Create default keys
                key.SetValue("Days", "5");
                key.SetValue("Resolve", "True");
                key.SetValue("Protocol", "http://");
                key.SetValue("Address", "AppV-Server");
                key.SetValue("Port", "8080");
                key.SetValue("Machine", "client3.local.lab");
                key.SetValue("Remote", "False");
                key.SetValue("Machine", "");
                key.SetValue("Username", Environment.UserName);
                key.SetValue("Domain", Environment.UserDomainName);
                key.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public bool CreateCredentialLocation()
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(regLocation);

                // Create default keys
                key.SetValue("Remote", "False");
                key.SetValue("Remember", "False");
                key.SetValue("Machine", "");
                key.SetValue("Username", Environment.UserName);
                key.SetValue("Domain", Environment.UserDomainName);
                key.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SaveSettings()
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.OpenSubKey(regLocation , RegistryKeyPermissionCheck.ReadWriteSubTree);

                key.SetValue("Days", ProgramSettings.Days.ToString());
                key.SetValue("Resolve", ProgramSettings.Resolve.ToString());
                key.SetValue("Remote", ProgramSettings.Remote.ToString());
                key.SetValue("Remember", ProgramSettings.Remember.ToString());
                key.SetValue("Machine", ProgramSettings.Machine);
                key.SetValue("Protocol", ProgramSettings.Protocol);
                key.SetValue("Address", ProgramSettings.Address);
                key.SetValue("Port", ProgramSettings.Port.ToString());
                key.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool LoadSettings()
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.OpenSubKey(regLocation, RegistryKeyPermissionCheck.ReadSubTree);

                if (key == null)
                {
                    CreateDefaultSettings();
                    key = Registry.CurrentUser.OpenSubKey(regLocation, RegistryKeyPermissionCheck.ReadSubTree);
                }

                ProgramSettings.Days = Convert.ToInt32(key.GetValue("Days").ToString());
                if (key.GetValue("Resolve").ToString() == "True")
                {
                    ProgramSettings.Resolve = true;
                }
                else
                {
                    ProgramSettings.Resolve = false;
                }

                ProgramSettings.Protocol = key.GetValue("Protocol").ToString();
                ProgramSettings.Address = key.GetValue("Address").ToString();
                ProgramSettings.Port = Convert.ToInt32(key.GetValue("Port").ToString());
                
                // Try to load new setting, otherwise create
                try
                {
                    key.GetValue("Remote").ToString();

                }
                catch (Exception)
                {
                    CreateCredentialLocation();                
                }
                
                if (key.GetValue("Remote").ToString() == "True")
                {
                    ProgramSettings.Remote = true;
                }
                else
                {
                    ProgramSettings.Remote = false;
                }

                if (key.GetValue("Remember").ToString() == "True")
                {
                    ProgramSettings.Remember = true;
                }
                else
                {
                    ProgramSettings.Remember = false;
                }

                ProgramSettings.Machine = key.GetValue("Machine").ToString();


                key.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool SaveCredentials()
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.OpenSubKey(regLocation, RegistryKeyPermissionCheck.ReadWriteSubTree);

                if (ProgramSettings.Remember)
                {
                    key.SetValue("Remember", ProgramSettings.Remember.ToString());
                    key.SetValue("Username", ProgramSettings.Username);
                    key.SetValue("Domain", ProgramSettings.Domain);
                    key.Close();
                }
                else
                {
                    key.SetValue("Remember", ProgramSettings.Remember.ToString());
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool LoadCredentials()
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.OpenSubKey(regLocation, RegistryKeyPermissionCheck.ReadSubTree);
    
                try
                {
                    key.GetValue("Username").ToString();
                }
                catch (Exception)
                {
                    CreateCredentialLocation();
                }

                ProgramSettings.Username = key.GetValue("Username").ToString();
                ProgramSettings.Domain = key.GetValue("Domain").ToString();

                key.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool SavePackage(string packageGuid, string versionGuid, string packageName, string verionNumber)
        {
            try
            {
                RegistryKey key;

                var packageRegLocation = regLocation + "\\Packages";
                key = Registry.CurrentUser.OpenSubKey(packageRegLocation, RegistryKeyPermissionCheck.ReadWriteSubTree);

                var keyName = packageGuid + "/" + versionGuid;
                var keyValue = packageName + "/" + verionNumber;
                
                key.SetValue(keyName, keyValue);

                key.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public string LoadPackage(string packageGuid, string versionGuid)
        {

            var packageName = "";

            try
            {
                RegistryKey key;
                var packageRegLocation = regLocation + "\\Packages";
                key = Registry.CurrentUser.OpenSubKey(packageRegLocation, RegistryKeyPermissionCheck.ReadSubTree);

                if (key == null)
                {
                    CreatePackageLocation();
                    key = Registry.CurrentUser.OpenSubKey(packageRegLocation, RegistryKeyPermissionCheck.ReadSubTree);
                }

                var keyName = packageGuid + "/" + versionGuid;

                packageName = key.GetValue(keyName).ToString();

                key.Close();
            }
            catch (Exception)
            {
                return packageName;
            }

            return packageName;
        }
    }
}

