using AppCommon.Helpers;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WeatherStation.UserControls
{
    /// <summary>
    /// Converts a temperature value and a TemperatureBar instance to a Brush using ColorHelper.
    /// </summary>
    public class TemperatureToBrushMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double temp && values[1] is TemperatureBar bar)
            {
                var color = ColorHelper.GetColorMultipoint(temp, bar.MinTemp, bar.MaxTemp);
                return new SolidColorBrush(color);
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }




    /// <summary>
    /// Represents a WPF UserControl for displaying a temperature bar.
    /// This control visualizes an array of temperature values within a specified minimum and maximum range.
    /// The color of each temperature segment is determined dynamically using the <see cref="ColorHelper.GetColorMultipoint"/> method,
    /// providing a visual gradient or mapping based on the temperature value.
    /// 
    /// <para>
    /// The control exposes three dependency properties:
    /// <list type="bullet">
    /// <item>
    /// <description><c>MinTemp</c>: The minimum temperature value displayed by the bar (default: -5).</description>
    /// </item>
    /// <item>
    /// <description><c>MaxTemp</c>: The maximum temperature value displayed by the bar (default: 25).</description>
    /// </item>
    /// <item>
    /// <description><c>Values</c>: An array of <see cref="double"/> representing the temperature values to be visualized.</description>
    /// </item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// The control is designed for use in weather or climate-related applications where a visual representation
    /// of temperature data is required. It supports data binding and can be integrated into XAML layouts.
    /// </para>
    /// </summary>
    public partial class TemperatureBar : UserControl
    {

        /// <summary>
        /// Dependency property registration for the <c>MinTemp</c> property.
        /// This property defines the minimum temperature value that can be displayed by the <see cref="TemperatureBar"/> control.
        /// It is a dependency property, allowing for data binding and property change notification within WPF.
        /// The default value is set to -5 degrees.
        /// </summary>
        protected static readonly DependencyProperty MinTempProperty = DependencyProperty.Register(
            "MinTemp",
            typeof(double),
            typeof(TemperatureBar),
            new PropertyMetadata(-5d));

        /// <summary>
        /// Gets or sets the minimum temperature value for the <see cref="TemperatureBar"/>.
        /// This property is a dependency property and supports data binding in XAML.
        /// </summary>
        public double MinTemp
        {
            get { return (double)GetValue(MinTempProperty); }
            set { SetValue(MinTempProperty, value); }
        }

        /// <summary>
        /// Dependency property registration for the <c>MaxTemp</c> property.
        /// This property defines the maximum temperature value that can be displayed by the <see cref="TemperatureBar"/> control.
        /// It is a dependency property, allowing for data binding and property change notification within WPF.
        /// The default value is set to +25 degrees.
        /// </summary>
        protected static readonly DependencyProperty MaxTempProperty = DependencyProperty.Register(
            "MaxTemp",
            typeof(double),
            typeof(TemperatureBar),
            new PropertyMetadata(+25d));

        /// <summary>
        /// Gets or sets the maximum temperature value for the <see cref="TemperatureBar"/>.
        /// This property is a dependency property and supports data binding in XAML.
        /// </summary>
        public double MaxTemp
        {
            get { return (double)GetValue(MaxTempProperty); }
            set { SetValue(MaxTempProperty, value); }
        }

        /// <summary>
        /// Dependency property registration for the <c>Values</c> property.
        /// This property holds an array of <see cref="double"/> representing the temperature values to be displayed by the <see cref="TemperatureBar"/> control.
        /// </summary>
        protected static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            "Values",
            typeof(double[]),
            typeof(TemperatureBar),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the array of temperature values displayed by the <see cref="TemperatureBar"/>.
        /// This property is a dependency property and supports data binding in XAML.
        /// </summary>
        public double[] Values
        {
            get { return (double[])GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }





        public TemperatureBar()
        {
            InitializeComponent();
        }
    }
}
