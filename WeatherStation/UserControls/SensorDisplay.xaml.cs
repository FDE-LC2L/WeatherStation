using AppCommon.Attributs;
using AppCommon.Extensions;
using AppCommon.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using WeatherStation.Model;

namespace WeatherStation.UserControls
{
    /// <summary>
    /// Logique d'interaction pour SensorDisplay.xaml
    /// </summary>
    public partial class SensorDisplay : UserControl
    {
        public class SensorDataCollection : List<SensorData>
        {
            public event EventHandler<SensorData>? SensorDataChanged;

            public void UpdateSensor(SensorData sensorData)
            {
                var existingSensor = Find(s => s.DeviceId == sensorData.DeviceId);
                if (existingSensor != null)
                {
                    existingSensor.CopyPropertiesFrom(sensorData);
                }
                else
                {
                    sensorData.PropertyChanged += SensorDataChange;
                    Add(sensorData);
                }
            }

            private void SensorDataChange(object? sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName == nameof(SensorData.DeviceId))
                {
                    SensorDataChanged?.Invoke(this, (SensorData)sender!);
                }

            }
        }

        private class SensorValue
        {
            public string DeviceId { get; set; } = string.Empty;
            public string PropertyName { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        private readonly SensorDataCollection _sensorDataCollection;

        public SensorDataCollection SensorDataList { get => _sensorDataCollection; }

        private readonly ObservableCollection<SensorValue> _sensorValues;


        public SensorDisplay()
        {
            InitializeComponent();
            _sensorDataCollection = new SensorDataCollection();
            _sensorDataCollection.SensorDataChanged += SensorDataList_SensorDataChanged;
            _sensorValues = new ObservableCollection<SensorValue>();
            ListViewSensor.ItemsSource = _sensorValues;
        }

        private void SensorDataList_SensorDataChanged(object? sender, SensorData sensorData)
        {
            var props = PropertyHelper.GetReadableProperties(sensorData, typeof(LabelAttribute));
            foreach (var prop in props)
            {
                var sensorValue = _sensorValues.FirstOrDefault(sv => sv.DeviceId == sensorData.DeviceId && sv.PropertyName == prop.Key);
                if (sensorValue != null)
                {
                    sensorValue.Value = prop.Value.ToString() ?? string.Empty;
                }
                else
                {
                    sensorValue = new SensorValue
                    {
                        DeviceId = sensorData.DeviceId,
                        PropertyName = prop.Key,
                        Value = prop.Value.ToString() ?? string.Empty
                    };
                    _sensorValues.Add(sensorValue);
                }
            }
            ListViewSensor.ItemsSource = null;
            ListViewSensor.ItemsSource = _sensorValues;

        }
    }
}
