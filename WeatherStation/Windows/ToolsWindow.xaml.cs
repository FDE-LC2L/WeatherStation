using KeyPad;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using WeatherStation.Api;
using WeatherStation.Infrastructure;
using WeatherStation.RemoteData.GeoApiCommunes;

namespace WeatherStation.Windows
{
    /// <summary>
    /// Logique d'interaction pour ToolsWindow.xaml
    /// </summary>
    public partial class ToolsWindow : BaseWindow
    {
        #region Fields
        public City? CurrentCity { get; set; }
        #endregion

        #region Ctor
        public ToolsWindow(Window owner, City? currentCity) : base(owner)
        {
            InitializeComponent();
            TextBlockWindowTitle.Text = AppResx.AppParameters.ToUpperInvariant();
            TextBoxPostalCode.Text = AppParameters.Settings.LastUsedPostalCode;
            CurrentCity = currentCity;
            SetComponents();
        }
        #endregion

        protected override void SetComponents()
        {
            base.SetComponents();
            ImageButtonSearchPostalCode.IsEnabled = TextBoxPostalCode.Text.Length == 5;
            ShowCityInfo(CurrentCity);
        }

        private void ShowCityInfo(City? city)
        {
            TextBlockCurrentCity.Text = city?.Name;
            TextBlockPostalCode.Text = city is object ? $"{AppResx.LibelPostalCode} : {city.PostalCodes[0]}" : string.Empty;
            if (city?.Center.coordinates.Count == 2)
            {
                TextBlockLatitude.Text = $"{AppResx.LibelLatitude} : {city.Center.coordinates[1].ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}";
                TextBlockLongitude.Text = $"{AppResx.LibelLongitude} : {city.Center.coordinates[0].ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}";
            }
            else
            {
                TextBlockLatitude.Text = string.Empty;
                TextBlockLongitude.Text = string.Empty;
            }
        }

        private async Task SearchByPostalCode(string postalCode)
        {
            AppParameters.Settings.LastUsedPostalCode = postalCode;
            var apiClient = new RestApiClient();
            var json = await apiClient.GetInfoCommunesAsync(postalCode);
            var cities = JsonSerializer.Deserialize<List<City>>(json);
            if (cities is object)
            {
                if (cities.Count == 0)
                {
                    CurrentCity = null;
                    MessageBox.Show(AppResx.NoCityFoundForThisPostalCode, AppResx.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (cities.Count == 1)
                {
                    CurrentCity = cities[0];
                }
                else
                {
                    var citiesWindow = new CitiesWindow(this, cities);
                    if (citiesWindow.ShowDialog() == true)
                    {
                        CurrentCity = citiesWindow.SelectedCity;
                    }
                }
                SetComponents();
            }
        }

        #region IHM
        private void ImageButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ImageButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private async void ImageButtonSearchPostalCode_Click(object sender, RoutedEventArgs e)
        {
            await SearchByPostalCode(TextBoxPostalCode.Text);
        }

        private void TextBoxPostalCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetComponents();
        }     

        private void TextBoxPostalCode_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void TextBoxPostalCode_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Keypad.Show(this, TextBoxPostalCode, Keypad.KeypadHorizontalAlignment.Center, Keypad.KeypadVerticalAlignment.Bottom);
        }       
        #endregion
    }
}
