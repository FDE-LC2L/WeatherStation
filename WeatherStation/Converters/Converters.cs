using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WeatherStation.Converters
{
    #region CustomConverter
    public abstract class CustomConverter : MarkupExtension, IValueConverter
    {
        public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }

    public abstract class CustomMultiValueConverter : MarkupExtension, IMultiValueConverter
    {
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }
    #endregion

    public class IsEmptyString : CustomConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }
    }

    public class CharToBool : CustomConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string == "O";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "O" : "N";
        }
    }

    public class ReversedBoolean : CustomConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

    }


    public class IsNotZero : CustomConverter
    {
        private static IsNotZero? _Instance;

        public static IsNotZero Instance => _Instance ??= new IsNotZero();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return float.TryParse(value.ToString(), out var result) && result != 0;
        }

    }

    public class IsZero : CustomConverter
    {
        private static IsZero? _Instance;

        public static IsZero Instance => _Instance ??= new IsZero();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !float.TryParse(value.ToString(), out var result) || result.Equals(0);
        }

    }

    public class Coalesce : CustomMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is object)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    if (values[i] is object && !string.IsNullOrWhiteSpace(values[i].ToString()))
                    {
                        return values[i].ToString()!;
                    }
                }
            }
            return string.Empty;
        }
    }

    public class TrimConverter : CustomConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valueString)
            {
                return valueString.Trim();
            }
            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class VisibilityConverter : CustomConverter
    {
        public Visibility True { get; set; }
        public Visibility False { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is object ? True : False;
        }
    }


    public class BinaryImageConverter : CustomConverter
    {
        public override object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is object && value is byte[])
            {
                var ByteArray = (value as byte[])!;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(ByteArray);
                bmp.EndInit();
                return bmp;
            }
            return null;
        }
    }

    public class ImageEnabledToOpacityConverter : CustomConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1 : 0.3;
        }
    }

    public class NameToColorConverter : CustomConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Color)ColorConverter.ConvertFromString((string)value);
        }
    }

    public class BoolToVisibilityConverter : CustomConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }
    }


}
