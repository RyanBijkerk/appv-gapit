using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GAP_IT.Models;

namespace GAP_IT.Interface.Charting
{

    public class ChartHandler
    {
        public ObservableCollection<string> ChartTypes { get; set; }

        public ObservableCollection<SeriesData> Series { get; set; }
        public ObservableCollection<DataClass> Items { get; set; }


        public ChartHandler(List<Timings> timingResultsList)
        {

            // Data format: StartTime, ResultTime, UserSID, PackgeID, VersionID, PackageName, Version, ResultSet
            // Index:       0,         1,          2,       3,        4,         5,           6,       7,

            // Add new serie and items set
            Series = new ObservableCollection<SeriesData>();
            Items = new ObservableCollection<DataClass>();

            // Set used variables
            var i = 0;
            var previousSet = 1;
            var newSerie = false;

            var dataSet = timingResultsList.ToArray();
            
            // Loop for adding data to the chart
            do
            {
                // Set used variables with data
                var appSet = dataSet[i].EventEntry1.EventSet;
                var appName = dataSet[i].EventEntry1.Package.Name;
                if (appName == "Unknown")
                    appName = dataSet[i].EventEntry1.Package.Guid;
                var appValue = dataSet[i].Timing;

                // See if new serie should be created
                if (previousSet != appSet)
                {
                    newSerie = true;
                }

                // If a new serie is created make new items
                if (newSerie)
                {
                    Items = new ObservableCollection<DataClass>();
                }

                // Add the data to the chart
                Items.Add(new DataClass() { Category = appName, Number = appValue });

                // Add the serie if needed
                if (newSerie)
                {
                    // Changed the series name to resultset id to avoid dublicate series names
                    // Series.Add(new SeriesData() { SeriesDisplayName = dataSet[i, 0], Items = Items });

                    Series.Add(new SeriesData() {SeriesDisplayName = dataSet[i].EventEntry1.EventSet.ToString() ,SeriesTitle = dataSet[i].EventEntry1.EventSet.ToString() + " - " + dataSet[i].EventEntry1.Time.ToString(), Items = Items});
                }

                // Set variable for next iteration
                previousSet = dataSet[i].EventEntry1.EventSet;
                newSerie = false;
                i++;
            } while (i <= dataSet.GetLength(0) - 1); 
        }

        public string ToolTipFormat
        {
            get { return "{0} has value '{1}'"; }
        }
    }

    public class SeriesData
    {
        public string SeriesDisplayName { get; set; }
        public string SeriesDescription { get; set; }
        public string SeriesTitle { get; set; }
        public ObservableCollection<DataClass> Items { get; set; }
    }

    public class DataClass : INotifyPropertyChanged
    {
        public string Category { get; set; }

        private float _number = 0;
        public float Number
        {
            get
            {
                return _number;
            }
            set
            {
                _number = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Number"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

