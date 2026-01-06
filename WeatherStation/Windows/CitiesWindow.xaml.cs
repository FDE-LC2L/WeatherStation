using System.Windows;
using System.Windows.Input;
using WeatherStation.Geo;
using WeatherStation.Infrastructure;

namespace WeatherStation.Windows
{
    /// <summary>
    /// Represents a window that displays a list of cities and allows the user to select one.
    /// Inherits from <see cref="BaseWindow"/> and provides functionality for city selection within a WPF application.
    /// </summary>
    public partial class CitiesWindow : BaseWindow
    {
        #region Fields
        public City? SelectedCity { get; private set; }
        #endregion

        #region Ctor
        public CitiesWindow()
        {
            InitializeComponent();
        }

        public CitiesWindow(Window owner, List<City> cities) : base(owner)
        {
            InitializeComponent();
            CitiesListBox.ItemsSource = cities;
        }
        #endregion

        /// <summary>
        /// Selects a city from the list based on the specified index, sets it as the selected city,
        /// updates the dialog result to true, and closes the window.
        /// </summary>
        /// <param name="index">The index of the city to select in the CitiesListBox.</param>
        private void SelectCity(int index)
        {
            SelectedCity = CitiesListBox.Items[index] as City;
            DialogResult = true;
            Close();
        }

        #region Events
        private void ImageButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedCity = null;
            DialogResult = false;
            Close();
        }

        private void CitiesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectCity(CitiesListBox.SelectedIndex);
        }
        #endregion


    }
}
