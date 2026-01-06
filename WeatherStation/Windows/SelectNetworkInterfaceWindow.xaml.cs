using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeatherStation.Infrastructure;
using static AppCommon.Helpers.NetworkHelper;

namespace WeatherStation.Windows
{
    public partial class SelectNetworkInterfaceWindow : BaseWindow
    {
        public NetworkInterfaceInfo? SelectedNetworkInterfaceInfo { get; private set; }

        public SelectNetworkInterfaceWindow(Window owner, List<NetworkInterfaceInfo> networkInterfaceInfos) : base(owner)
        {
            InitializeComponent();
            SetWindowsMode();
            WindowState = WindowState.Maximized;
            ListViewNetworkInterfaces.Items.Clear();
            ListViewNetworkInterfaces.ItemsSource = networkInterfaceInfos;
        }

        /// <summary>
        /// Adjusts the width of the "Description" column to occupy the remaining space
        /// after accounting for the widths of other columns in the ListView.
        /// </summary>
        private void AdjustDescriptionColumnWidths()
        {
            // Get the total width of the ListView
            double totalWidth = ListViewNetworkInterfaces.ActualWidth;
            // Check if the ListView has a GridView as its View
            if (ListViewNetworkInterfaces.View is GridView gridView)
            {
                // Calculate the total width of all columns except the "Description" column
                double otherColumnsWidth = 0;
                foreach (var column in gridView.Columns)
                {
                    // Skip the "Description" column
                    if (column != DescriptionColumn)
                    {
                        otherColumnsWidth += column.Width;
                    }
                }
                // Calculate the remaining width for the "Description" column
                double remainingWidth = totalWidth - otherColumnsWidth;
                // Set the width of the "Description" column if the remaining width is positive
                if (remainingWidth > 0)
                {
                    DescriptionColumn.Width = remainingWidth;
                }
            }
        }

        #region IHM
        private void ListViewNetworkInterfaces_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustDescriptionColumnWidths();
        }

        private void ListViewNetworkInterfaces_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is NetworkInterfaceInfo selectedNetworkInterfaceInfo)
            {
                SelectedNetworkInterfaceInfo = selectedNetworkInterfaceInfo;
                DialogResult = true;
                Close();
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion
    }
}
