using AppCommon.Attributs;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows;

namespace WeatherStation.Model
{
    public class SensorData : DependencyObject
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Fields
        private string _deviceId = string.Empty;

        private bool _isTempSensorOk;

        private float _temperature;

        private float _humidity;

        private bool _isPressureSensorOk;

        private float _pressure;

        [JsonPropertyName("did")]
        public string DeviceId { get => _deviceId; set => SetDeviceId(value); }
        [JsonPropertyName("tok")]
        public bool IsTempSensorOk { get => _isTempSensorOk; set => SetIsTempSensorOk(_isTempSensorOk); }
        [JsonPropertyName("tem")]
        public float Temperature { get => _temperature; set => SetTemperature(value); }
        [JsonPropertyName("hum")]
        public float Humidity { get => _humidity; set => SetHumidity(value); }
        [JsonPropertyName("pok")]
        public bool IsPressureSensorOk { get => _isPressureSensorOk; set => SetIsPressureSensorOk(value); }
        [JsonPropertyName("pre")]
        public float Pressure { get => _pressure; set => SetPressure(value); }
        public DateTime DataDateTime { get; set; }

        [JsonIgnore]
        [Label("Température")]
        public string FormattedTemperature { get => Temperature.ToString("F2") + "°C"; }
        [JsonIgnore]
        [Label("Humidité")]
        public string FormattedHumidity { get => Humidity.ToString("F2") + "%"; }
        [JsonIgnore]
        [Label("Pression")]
        public string FormattedPressure { get => Pressure.ToString("F2") + " hPa"; }
        [JsonIgnore]
        [Label("Heure")]
        public string FormattedDataDateTime { get => (DataDateTime == DateTime.MinValue) ? string.Empty : DataDateTime.ToString("dd/MM/yyyy HH:mm:ss"); }


        // Setters regrouped here
        public void SetDeviceId(string value)
        {
            _deviceId = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceId)));
        }
        public void SetIsTempSensorOk(bool value)
        {
            _isTempSensorOk = value;
            DataDateTime = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTempSensorOk)));
        }
        public void SetTemperature(float value)
        {
            _temperature = value;
            DataDateTime = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
        }
        public void SetHumidity(float value)
        {
            _humidity = value;
            DataDateTime = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Humidity)));
        }
        public void SetIsPressureSensorOk(bool value)
        {
            _isPressureSensorOk = value;
            DataDateTime = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPressureSensorOk)));
        }
        public void SetPressure(float value)
        {
            _pressure = value;
            DataDateTime = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pressure)));
        }
        #endregion
    }
}
