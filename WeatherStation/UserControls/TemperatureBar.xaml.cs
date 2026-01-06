using AppCommon.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    /// Logique d'interaction pour TemperatureBar.xaml
    /// </summary>
    public partial class TemperatureBar : UserControl
    {

        protected static readonly DependencyProperty MinTempProperty = DependencyProperty.Register(
            "MinTemp",
            typeof(double),
            typeof(TemperatureBar),
            new PropertyMetadata(-5d));

        public double MinTemp
        {
            get { return (double)GetValue(MinTempProperty); }
            set { SetValue(MinTempProperty, value); }
        }


        protected static readonly DependencyProperty MaxTempProperty = DependencyProperty.Register(
            "MaxTemp",
            typeof(double),
            typeof(TemperatureBar),
            new PropertyMetadata(+25d));

        public double MaxTemp
        {
            get { return (double)GetValue(MaxTempProperty); }
            set { SetValue(MaxTempProperty, value); }
        }

        protected static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            "Values",
            typeof(double[]),
            typeof(TemperatureBar),
            new PropertyMetadata(null));

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
