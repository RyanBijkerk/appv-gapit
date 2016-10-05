using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GAP_IT.Code;
using GAP_IT.Interface.Charting;
using GAP_IT.Models;

namespace GAP_IT.Interface
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        private bool featureRemoteComputer = true;
        
        // Set default port value
        private int _numValue = 8080;
        private RegistryHandler registrySettings = new RegistryHandler();
        private List<Timings> DataSet { get; set; }

        public int NumValue
        { get { return _numValue; } set { _numValue = value; portTextBox.Text = value.ToString(); } }

        public Main()
        {
            InitializeComponent();

            // Set version number in header
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            mainWindow.Title = "GAP-IT " + version + " by Ryan Bijkerk ";

            // Don't show the loading spinner
            loadingLabel.Visibility = Visibility.Hidden;
            loadingSpinner.Visibility = Visibility.Hidden;
            
            // Try load setting
            var loaded = registrySettings.LoadSettings();
            
            // If failed create new settings in regitry
            if (!loaded)
            {
                registrySettings.CreateDefaultSettings();
                registrySettings.CreatePackageLocation();
                registrySettings.LoadSettings();
            }
            
            // enable or disable features
            if (!featureRemoteComputer)
            {
                labelRemoteComputer.Visibility = Visibility.Hidden;
                labelRemoteConfig.Visibility = Visibility.Hidden;
                radioNo_machine.Visibility = Visibility.Hidden;
                radioYes_machine.Visibility = Visibility.Hidden;
                remoteComputer.Visibility = Visibility.Hidden;

            }
            
            // Apply settings in the GUI
            ApplySettings();

            // Set error bar hidden
            errorBorder.Visibility = Visibility.Hidden;
            errorLabel.Visibility = Visibility.Hidden;
        }

        private void info_Click(object sender, RoutedEventArgs e)
        {
            About aboutPage = new About();
            aboutPage.ParentControl = this.dockAbout;
            dockAbout.Children.Clear();
            dockAbout.Children.Add(aboutPage);
            
        }

        public void info_ClickClose(object sender, RoutedEventArgs e)
        {
            dockAbout.Children.Clear();
        }

        private void collectData_Click(object sender, RoutedEventArgs e)
        {
            var saved = SaveSettings();
            ApplySettings();

            loadingLabel.Visibility = Visibility.Visible;
            loadingSpinner.Visibility = Visibility.Visible;

            if (saved)
            {
                var logons = new LogonEvents();
                var appv = new EventAppV();

                collectData.IsEnabled = false;

                Thread collectDataThread = new Thread(() =>
                {
                    //var logonResults = logons.Collect();
                    
                    var appvResults = appv.Collect();

                    if (appvResults.GetLength(0) == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            errorBorder.Visibility = Visibility.Visible;
                            errorLabel.Visibility = Visibility.Visible;
                            errorLabel.Content = "No results found!";
                        });
                    }

                    if (appvResults.GetLength(0) != 0)
                    {
                        // create resulthandler
                        var data = new ResultHandler();

                        // collect logon events (not used)
                        // data.Logon = logonResults;

                        // collect App-V related events
                        data.AppV = appvResults;

                        // build the set with results
                        var results = data.BuildSet();
                        // create results list with timings
                        DataSet = data.Parse(results);

                        // update the chart with the results
                        Update(DataSet);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            collectData.IsEnabled = true;
                        });
                    }
                });

                if (ProgramSettings.Remote)
                {
                    Credentials credentialsWindow = new Credentials();
                    credentialsWindow.ShowDialog();
                }

                collectDataThread.Start();
                
            }
        }

        private void port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (portTextBox == null)
            {
                return;
            }

            if (!int.TryParse(portTextBox.Text, out _numValue))
                portTextBox.Text = _numValue.ToString();
        }

        private void port_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;

            }
        }

        private void serverTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (serverTextBox.Text == "AppV-Server")
            {
                serverTextBox.Clear();
            }
        }

        private void radioNo_Machine_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remoteComputer.IsEnabled)
                {
                    remoteComputer.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private void radioYes_Machine_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!remoteComputer.IsEnabled)
                {
                    remoteComputer.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private void radioYes_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!protocol.IsEnabled)
                {
                    protocol.IsEnabled = true;
                }

                if (!serverTextBox.IsEnabled)
                {
                    serverTextBox.IsEnabled = true;
                }

                if (!portTextBox.IsEnabled)
                {
                    portTextBox.IsEnabled = true;
                }

                if (!protocol.IsEnabled)
                {
                    protocol.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private void radioNo_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (protocol.IsEnabled)
                {
                    protocol.IsEnabled = false;
                }

                if (serverTextBox.IsEnabled)
                {
                    serverTextBox.IsEnabled = false;
                }

                if (portTextBox.IsEnabled)
                {
                    portTextBox.IsEnabled = false;
                }

                if (protocol.IsEnabled)
                {
                    protocol.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        public void ApplySettings()
        {
            datePicker.SelectedDate = DateTime.Now.AddDays(-ProgramSettings.Days);     

            if (ProgramSettings.Resolve)
            {
                radioYes.IsChecked = true;
            }
            else
            {
                radioNo.IsChecked = true;

            }

            remoteComputer.Text = ProgramSettings.Machine;
            protocol.Text = ProgramSettings.Protocol;
            serverTextBox.Text = ProgramSettings.Address;
            portTextBox.Text = ProgramSettings.Port.ToString();
            remoteComputer.Text = ProgramSettings.Machine;

            if (ProgramSettings.Remote)
            {
                radioYes_machine.IsChecked = true;
            }
            else
            {
                radioNo_machine.IsChecked = true;

            }
        }
        
        public bool SaveSettings()
        {

            errorBorder.Visibility = Visibility.Hidden;
            errorLabel.Visibility = Visibility.Hidden;

            var port = Convert.ToInt32(portTextBox.Text);
            if (datePicker.SelectedDate >= DateTime.Now)
            {
                errorBorder.Visibility = Visibility.Visible;
                errorLabel.Visibility = Visibility.Visible;
                errorLabel.Content = "Selected date is now or in the future";
                return false;
            }
            var days = Convert.ToInt32(DateTime.Now.Date.Subtract(datePicker.SelectedDate.Value).TotalDays);
            ProgramSettings.Days = days;

            if (radioYes.IsChecked.Value)
            {
                ProgramSettings.Resolve = true;
                if (serverTextBox.Text == "")
                {
                    errorBorder.Visibility = Visibility.Visible;
                    errorLabel.Visibility = Visibility.Visible;
                    errorLabel.Content = "App-V management server cannot be empty";
                    return false;
                }
            }
            else
            {
                ProgramSettings.Resolve = false;
            }

            if (radioYes_machine.IsChecked.Value)
            {
                ProgramSettings.Remote = true;
                if (remoteComputer.Text == "")
                {
                    errorBorder.Visibility = Visibility.Visible;
                    errorLabel.Visibility = Visibility.Visible;
                    errorLabel.Content = "Remote computer cannot be empty";
                    return false;
                }
            }
            else
            {
                ProgramSettings.Remote = false;
            }

            ProgramSettings.Machine = remoteComputer.Text;
            ProgramSettings.Protocol = protocol.Text;
            ProgramSettings.Address = serverTextBox.Text;
            ProgramSettings.Port = port;

            registrySettings.SaveSettings();
            
            return true;
        }

        public void Update(List<Timings> dataSet)
        {
            Dispatcher.Invoke(() => {
                                        this.DataContext = new ChartHandler(dataSet);
                                        collectData.IsEnabled = true;
                                        loadingLabel.Visibility = Visibility.Hidden;
                                        loadingSpinner.Visibility = Visibility.Hidden;
                                        exportButton.IsEnabled = true;
            });
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV file (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            var data = new ResultHandler();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                var deleteExeption = false;
                if (System.IO.File.Exists(filename))
                {
                    try
                    {
                        System.IO.File.Delete(filename);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Cannot delete file: " + filename);
                        deleteExeption = true;
                    }
                }
                try
                {
                    if (!deleteExeption)
                    {
                        data.Export(filename, DataSet);
                    }
                }
                catch (Exception)
                {
                    errorLabel.Visibility = Visibility.Visible;
                    errorLabel.Content = "Cannot edit the file: " + filename;
                }
            }
        }
    }
}