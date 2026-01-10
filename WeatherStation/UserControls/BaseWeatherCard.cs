using AppCommon.Extensions;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WeatherStation.WeatherData.InfoClimat;

namespace WeatherStation.UserControls
{

    #region Internal Converters

    /// <summary>
    /// Value converter for image height in WPF bindings.
    /// This converter returns the original value if it is less than or equal to 100,
    /// otherwise it returns a fixed value of 250.
    /// Used to limit the maximum height of an image in the user interface.
    /// </summary>
    public class ImageHeightConverter : IValueConverter
    {
        /// <summary>
        /// Converts the input value to a height for an image.
        /// If the value is less than or equal to 100, returns the value itself.
        /// Otherwise, returns 250.
        /// </summary>
        /// <param name="value">The value to convert, expected to be a double representing the height.</param>
        /// <param name="targetType">The target type of the binding (not used).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>
        /// The original value if it is less than or equal to 100; otherwise, 250.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value <= 100) ? (double)value : 250;
        }

        /// <summary>
        /// Not implemented. Throws a NotImplementedException if called.
        /// </summary>
        /// <param name="value">The value produced by the binding target (not used).</param>
        /// <param name="targetType">The type to convert to (not used).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>None. Always throws an exception.</returns>
        /// <exception cref="NotImplementedException">Always thrown when this method is called.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Value converter for displaying a <see cref="TimeOnly"/> value as a formatted string with hours and minutes (e.g., "14:05").
    /// This converter is intended for use in WPF bindings, such as displaying the time in a <see cref="TextBlock"/>.
    /// </summary>
    public class TimeOnlyToHourMinuteConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="TimeOnly"/> value to a string in "HH:mm" format.
        /// </summary>
        /// <param name="value">The value to convert, expected to be a <see cref="TimeOnly"/> or nullable <see cref="TimeOnly"/>.</param>
        /// <param name="targetType">The target type of the binding (not used).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>
        /// A string representing the time in "HH:mm" format, or an empty string if the value is null or not a <see cref="TimeOnly"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeOnly time)
            {
                return time.ToString("HH:mm");
            }
            return string.Empty;
        }

        /// <summary>
        /// Not implemented. Throws a <see cref="NotImplementedException"/> if called.
        /// </summary>
        /// <param name="value">The value produced by the binding target (not used).</param>
        /// <param name="targetType">The type to convert to (not used).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>None. Always throws an exception.</returns>
        /// <exception cref="NotImplementedException">Always thrown when this method is called.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    public class BaseWeatherCard : UserControl
    {

        #region Dependency Properties

        /// <summary>
        /// Dependency property for the <see cref="InfoClimat"/> property.
        /// Represents the InfoClimatManager instance used to retrieve weather data for the CurrentWeatherCard.
        /// </summary>
        public static readonly DependencyProperty InfoClimatManagerProperty = DependencyProperty.Register(
            "InfoClimat",
            typeof(InfoClimatManager),
            typeof(BaseWeatherCard),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the InfoClimatManager instance for the CurrentWeatherCard1.
        /// This property is a dependency property and is used to bind the InfoClimatManager to the control.
        /// </summary>
        public InfoClimatManager? InfoClimat
        {
            get { return (InfoClimatManager)GetValue(InfoClimatManagerProperty); }
            set { SetValue(InfoClimatManagerProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="ForecastDate"/> property.
        /// Represents the date of the forecast displayed in the FutureWeatherCard.
        /// </summary>
        public static readonly DependencyProperty ForecastDateProperty = DependencyProperty.Register(
            "ForecastDate",
            typeof(DateOnly),
            typeof(BaseWeatherCard),
            new PropertyMetadata(default(DateOnly)));

        /// <summary>
        /// Gets or sets the date of the forecast for the FutureWeatherCard.
        /// This property is a dependency property and is used to bind the forecast date to the control.
        /// </summary>
        public DateOnly ForecastDate
        {
            get { return (DateOnly)GetValue(ForecastDateProperty); }
            set { SetValue(ForecastDateProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="CloudinessText"/> property.
        /// Represents the textual description of the cloudiness level displayed in the FutureWeatherCard.
        /// </summary>
        protected static readonly DependencyProperty CloudinessTextProperty = DependencyProperty.Register(
            "CloudinessText",
            typeof(string),
            typeof(BaseWeatherCard),
            new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the textual description of the cloudiness level for the FutureWeatherCard.
        /// This property is a dependency property and is used internally to bind the cloudiness text to the control.
        /// </summary>
        protected string CloudinessText
        {
            get { return (string)GetValue(CloudinessTextProperty); }
            set { SetValue(CloudinessTextProperty, value); }
        }

        protected static readonly DependencyProperty MaxTempProperty = DependencyProperty.Register(
            "MaxTemp",
            typeof(double),
            typeof(BaseWeatherCard),
            new PropertyMetadata(0d));

        /// <summary>
        /// Gets or sets the maximum temperature for the forecast day displayed in the weather card.
        /// This property is a dependency property used internally to bind the maximum temperature value to the control.
        /// The temperature value is expected to be in degrees Celsius.
        /// </summary>
        protected double MaxTemp
        {
            get { return (double)GetValue(MaxTempProperty); }
            set { SetValue(MaxTempProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="MinTemp"/> property.
        /// Represents the minimum temperature for the forecast day displayed in the weather card.
        /// </summary>
        protected static readonly DependencyProperty MinTempProperty = DependencyProperty.Register(
            "MinTemp",
            typeof(double),
            typeof(BaseWeatherCard),
            new PropertyMetadata(0d));

        /// <summary>
        /// Gets or sets the minimum temperature for the forecast day displayed in the weather card.
        /// This property is a dependency property used internally to bind the minimum temperature value to the control.
        /// The temperature value is expected to be in degrees Celsius.
        /// </summary>
        protected double MinTemp
        {
            get { return (double)GetValue(MinTempProperty); }
            set { SetValue(MinTempProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="DayName"/> property.
        /// Represents the localized name of the day of the week for the forecast date displayed in the weather card.
        /// </summary>
        protected static readonly DependencyProperty DayNameProperty = DependencyProperty.Register(
            "DayName",
            typeof(string),
            typeof(BaseWeatherCard),
            new PropertyMetadata("DayName"));

        /// <summary>
        /// Gets or sets the localized name of the day of the week for the forecast date.
        /// This property is a dependency property used internally to bind the day name to the control.
        /// The value is typically obtained from <see cref="ForecastDate"/> using culture-specific formatting.
        /// </summary>
        protected string DayName
        {
            get { return (string)GetValue(DayNameProperty); }
            set { SetValue(DayNameProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="FormattedDate"/> property.
        /// Represents the formatted date string for the forecast displayed in the weather card.
        /// This property is used internally to bind a culture-specific formatted date to the control,
        /// typically in the format "d MMM" (e.g., "4 Jan").
        /// </summary>
        protected static readonly DependencyProperty FormattedDateProperty = DependencyProperty.Register(
            "FormattedDate",
            typeof(string),
            typeof(BaseWeatherCard),
            new PropertyMetadata("FormattedDate"));

        /// <summary>
        /// Gets or sets the formatted date string for the forecast.
        /// This property is a dependency property used internally to bind the formatted date to the control.
        /// The value is typically set using culture-specific formatting based on the <see cref="ForecastDate"/>.
        /// </summary>
        protected string FormattedDate
        {
            get { return (string)GetValue(FormattedDateProperty); }
            set { SetValue(FormattedDateProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="SunImageSrc"/> property.
        /// Represents the image source for the weather condition icon displayed in the weather card.
        /// </summary>
        protected static readonly DependencyProperty SunImageSrcProperty = DependencyProperty.Register(
            "SunImageSrc",
            typeof(ImageSource),
            typeof(BaseWeatherCard),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the image source for the weather condition icon.
        /// This property is a dependency property used internally to bind the weather icon to the control.
        /// The image is typically selected based on cloudiness level and retrieved from application resources.
        /// </summary>
        protected ImageSource SunImageSrc
        {
            get { return (ImageSource)GetValue(SunImageSrcProperty); }
            set { SetValue(SunImageSrcProperty, value); }
        }


        /// <summary>
        /// Dependency property for the <see cref="RainImageSrc"/> property.
        /// Represents the image source for the rain condition icon displayed in the weather card.
        /// This property is used internally to bind the rain icon to the control.
        /// The image is typically selected based on the rain intensity and retrieved from application resources.
        /// </summary>
        protected static readonly DependencyProperty RainImageSrcProperty = DependencyProperty.Register(
            "RainImageSrc",
            typeof(ImageSource),
            typeof(BaseWeatherCard),
            new FrameworkPropertyMetadata(null));


        /// <summary>
        /// Gets or sets the image source for the rain condition icon.
        /// This property is a dependency property used internally to bind the rain icon to the control.
        /// The image is typically selected based on the rain intensity and retrieved from application resources.
        /// </summary>
        protected ImageSource RainImageSrc
        {
            get { return (ImageSource)GetValue(RainImageSrcProperty); }
            set { SetValue(RainImageSrcProperty, value); }
        }


        /// <summary>
        /// Dependency property for the <see cref="WindImageSrc"/> property.
        /// Represents the image source for the weather condition icon displayed in the weather card.
        /// </summary>
        protected static readonly DependencyProperty WindImageSrcProperty = DependencyProperty.Register(
            "WindImageSrc",
            typeof(ImageSource),
            typeof(BaseWeatherCard),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the image source for the wind condition icon.
        /// This property is a dependency property used internally to bind the wind icon to the control.
        /// The image is typically selected based on wind conditions and retrieved from application resources.
        /// </summary>
        protected ImageSource WindImageSrc
        {
            get { return (ImageSource)GetValue(WindImageSrcProperty); }
            set { SetValue(WindImageSrcProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="WindOrientation"/> property.
        /// Represents the orientation of the wind, typically as an angle in degrees, displayed in the weather card.
        /// This property is used internally to bind the wind orientation value to the control.
        /// The value is expected to be an integer corresponding to the wind direction.
        /// </summary>
        protected static readonly DependencyProperty WindOrientationProperty = DependencyProperty.Register(
            "WindOrientation",
            typeof(int),
            typeof(BaseWeatherCard),
            new FrameworkPropertyMetadata(0));

        /// <summary>
        /// Gets or sets the orientation of the wind, typically as an angle in degrees, for the weather card.
        /// This property is a dependency property used internally to bind the wind orientation value to the control.
        /// The value is expected to be an integer corresponding to the wind direction.
        /// </summary>
        protected int WindOrientation
        {
            get { return (int)GetValue(WindOrientationProperty); }
            set { SetValue(WindOrientationProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="WindColor"/> property.
        /// Represents the brush used to display the wind color in the weather card.
        /// This property is used internally to bind the wind color to the control,
        /// allowing dynamic color changes based on wind speed or other criteria.
        /// The default value is set to <see cref="Brushes.Blue"/>.
        /// </summary>
        protected static readonly DependencyProperty WindColorProperty = DependencyProperty.Register(
            "WindColor",
            typeof(Brush),
            typeof(BaseWeatherCard),
            new FrameworkPropertyMetadata(Brushes.Blue));

        /// <summary>
        /// Gets or sets the brush used to display the wind color in the weather card.
        /// This property is a dependency property used internally to bind the wind color to the control,
        /// allowing dynamic color changes based on wind speed or other criteria.
        /// The default value is set to <see cref="Brushes.Blue"/>.
        /// </summary>
        protected Brush WindColor
        {
            get { return (Brush)GetValue(WindColorProperty); }
            set { SetValue(WindColorProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="ForcastTime"/> property.
        /// Represents the specific time of the forecast displayed in the weather card.
        /// This property is used to store and retrieve the time at which the forecast data applies,
        /// allowing the card to display weather information for a particular hour of the day.
        /// </summary>
        protected static readonly DependencyProperty ForcastTimeProperty = DependencyProperty.Register(
            "ForcastTime",
            typeof(TimeOnly?),
            typeof(BaseWeatherCard),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the specific time of the forecast displayed in the weather card.
        /// This property is a dependency property that represents the hour at which the forecast data applies.
        /// It is nullable to allow scenarios where no specific time has been set or retrieved.
        /// The value is typically set when updating the card with forecast data for a particular time of day.
        /// </summary>
        /// <value>
        /// A nullable <see cref="TimeOnly"/> representing the forecast time, or null if no time has been specified.
        /// </value>
        public TimeOnly? ForcastTime
        {
            get { return (TimeOnly?)GetValue(ForcastTimeProperty); }
            set { SetValue(ForcastTimeProperty, value); }
        }
        #endregion

        /// <summary>
        /// Updates the weather card with a new <see cref="InfoClimatManager"/> instance.
        /// This method assigns the provided <paramref name="infoClimat"/> to the <see cref="InfoClimat"/> property,
        /// then calls <see cref="UpdateCard()"/> to refresh the card's display with the latest weather data.
        /// </summary>
        /// <param name="infoClimat">
        /// The <see cref="InfoClimatManager"/> instance containing the weather data to be displayed.
        /// </param>
        public void UpdateCard(InfoClimatManager infoClimat)
        {
            InfoClimat = infoClimat;
            UpdateCard();
        }

        /// <summary>
        /// Updates the FutureWeatherCard with the latest forecast data for the specified date and time.
        /// </summary>
        public void UpdateCard()
        {
            if (InfoClimat is object)
            {
                SetDayTemperature(InfoClimat);
                ForecastData? forecast;
                var hourOffset = 0;
                var forecastHour = ForcastTime is not null ? (InfoClimatManager.Hour)ForcastTime.Value.Hour : InfoClimatManager.Hour.Fourteen;
                /// <summary>
                /// Attempts to retrieve the weather forecast for the specified date and time, starting from 14:00 (Hour.Fourteen).
                /// If no forecast is found at 14:00, the method alternates by incrementally searching later and earlier hours,
                /// increasing the offset by one each iteration, up to a maximum offset of 10 hours.
                /// This approach ensures that the closest available forecast to 14:00 is selected, prioritizing later hours first.
                /// </summary>
                do
                {
                    forecast = InfoClimat.GetForecast(ForecastDate, forecastHour + hourOffset);
                    if (forecast is null && hourOffset != 0)
                    {
                        forecast = InfoClimat.GetForecast(ForecastDate, forecastHour - hourOffset);
                    }
                    hourOffset++;
                } while (forecast is null && hourOffset <= 10);

                if (forecast is object)
                {
                    ForcastTime = TimeOnly.FromDateTime(DateTime.Parse(forecast.DateString));
                    CloudinessText = GetCloudinessText(forecast.Cloudiness!.Total);
                    DayName = ForecastDate.GetDayOfWeek(CultureInfo.CurrentCulture);
                    FormattedDate = ForecastDate.ToString("d MMM", CultureInfo.CurrentCulture);
                    SunImageSrc = (ImageSource)Application.Current.Resources[GetCloudinessImageName(forecast.Cloudiness!.Total)];
                    RainImageSrc = (ImageSource)Application.Current.Resources[GetRainImageName(forecast.Rain)];
                    WindImageSrc = (ImageSource)Application.Current.Resources[GetWindImageName()];
                    WindOrientation = Convert.ToInt32(forecast?.WindDirection?.At10m ?? 0);
                    WindColor = GetWindColor(forecast?.WindAverage?.At10m ?? 0);
                }
            }
        }

        /// <summary>
        /// Converts a cloudiness value into a textual description based on predefined thresholds.
        /// </summary>
        /// <param name="cloudinessValue">
        /// The cloudiness value as an integer, typically representing a percentage (0 to 100).
        /// </param>
        /// <returns>
        /// A string representing the cloudiness description.
        /// </returns>
        protected static string GetCloudinessText(int cloudinessValue)
        {
            return cloudinessValue switch
            {
                >= 80 => AppResx.Cloudiness_FullyCovered,
                >= 50 and < 80 => AppResx.Cloudiness_Covered,
                >= 25 and < 50 => AppResx.Cloudiness_Cloudy,
                >= 15 and < 25 => AppResx.Cloudiness_LittleCovered,
                _ => AppResx.Cloudiness_Sunny
            };
        }

        /// <summary>
        /// Returns the resource name of the weather icon corresponding to the specified cloudiness value.
        /// The method uses predefined thresholds to map the cloudiness percentage to a specific image resource name.
        /// </summary>
        /// <param name="cloudinessValue">
        /// The cloudiness value as an integer, typically representing a percentage (0 to 100).
        /// </param>
        /// <returns>
        /// The name of the image resource corresponding to the cloudiness level.
        /// </returns>
        protected static string GetCloudinessImageName(int cloudinessValue)
        {
            return cloudinessValue switch
            {
                >= 80 => "imgHardCloudy",
                >= 50 and < 80 => "imgCloudy",
                >= 25 and < 50 => "imgPartlyCloudy",
                >= 15 and < 25 => "imgMostlySunny",
                _ => "imgClearDay"
            };
        }

        /// <summary>
        /// Returns the resource name of the rain icon corresponding to the specified precipitation value.
        /// The method uses predefined thresholds to map the rain amount (in millimeters) to a specific image resource name.
        /// </summary>
        /// <param name="rainValue">
        /// The rain value as a double, typically representing the amount of precipitation in millimeters.
        /// </param>
        /// <returns>
        /// The name of the image resource corresponding to the rain intensity level.
        /// </returns>
        protected static string GetRainImageName(double rainValue)
        {
            return rainValue switch
            {
                >= 10 => "imgVeryHeavyRain",
                >= 5 and < 10 => "imgHeavyRain",
                >= 2 and < 5 => "imgModerateRain",
                >= 0.2 and < 2 => "imgLightRain",
                _ => "imgNoRain"
            };
        }

        /// <summary>
        /// Returns the resource name of the wind icon to be displayed in the weather card.
        /// This method always returns the name of the corresponds to the wind image resource
        /// used in the application's resources for representing wind conditions.
        /// </summary>
        protected static string GetWindImageName()
        {
            return "imgWind";
        }

        /// <summary>
        /// Returns a <see cref="Brush"/> representing the wind color based on the provided wind speed in kilometers per hour.
        /// The color is interpolated according to the following ranges:
        /// - 0 to 10 km/h: White to Yellow
        /// - 10 to 30 km/h: Yellow to Orange
        /// - 30 to 60 km/h: Orange to Red
        /// - Above 60 km/h: Red
        /// The method uses linear interpolation between the color ranges to provide a smooth gradient effect.
        /// </summary>
        /// <param name="windSpeedKmH">
        /// The wind speed in kilometers per hour.
        /// </param>
        /// <returns>
        /// A <see cref="SolidColorBrush"/> with the color corresponding to the wind speed.
        /// </returns>
        public static Brush GetWindColor(double windSpeedKmH)
        {
            var (r, g, b) = windSpeedKmH switch
            {
                <= 10 => InterpolateWindColor(windSpeedKmH, 0, 10, (255, 255, 255), (255, 255, 0)),   // White -> Yellow
                <= 30 => InterpolateWindColor(windSpeedKmH, 10, 30, (255, 255, 0), (255, 165, 0)),    // Yellow -> Orange
                <= 60 => InterpolateWindColor(windSpeedKmH, 30, 60, (255, 165, 0), (255, 0, 0)),      // Orange -> Red
                _ => (255, 0, 0)                                                                      // Red
            };
            return new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
        }

        /// <summary>
        /// Performs a linear interpolation between two RGB color values based on the specified value and range.
        /// This method is used to calculate an intermediate color for wind speed visualization,
        /// providing a smooth gradient between the start and end colors according to the wind speed.
        /// </summary>
        /// <param name="valeur">
        /// The value for which the color interpolation is performed, typically representing wind speed.
        /// </param>
        /// <param name="min">
        /// The minimum value of the interpolation range.
        /// </param>
        /// <param name="max">
        /// The maximum value of the interpolation range.
        /// </param>
        /// <param name="startColor">
        /// The RGB color tuple representing the start color of the interpolation range.
        /// </param>
        /// <param name="endColor">
        /// The RGB color tuple representing the end color of the interpolation range.
        /// </param>
        /// <returns>
        /// A tuple containing the interpolated RGB color values as bytes.
        /// </returns>
        private static (byte r, byte g, byte b) InterpolateWindColor(double valeur, double min, double max,
                                                (byte r, byte g, byte b) startColor,
                                                (byte r, byte g, byte b) endColor)
        {
            double ratio = (valeur - min) / (max - min);

            byte r = (byte)(startColor.r + ratio * (endColor.r - startColor.r));
            byte g = (byte)(startColor.g + ratio * (endColor.g - startColor.g));
            byte b = (byte)(startColor.b + ratio * (endColor.b - startColor.b));

            return (r, g, b);
        }

        /// <summary>
        /// Calculates and sets the minimum and maximum temperatures for the forecast day.
        /// Iterates through all available hours defined in <see cref="InfoClimatManager.Hour"/>,
        /// retrieves the temperature at 2 meters for each hour (in Kelvin), converts it to Celsius,
        /// and updates the minimum and maximum temperature values accordingly.
        /// The results are assigned to the <see cref="MinTemp"/> and <see cref="MaxTemp"/> dependency properties.
        /// </summary>
        /// <param name="infoClimatManager">
        /// The <see cref="InfoClimatManager"/> instance used to retrieve forecast data for the specified date and hours.
        /// </param>
        protected void SetDayTemperature(InfoClimatManager infoClimatManager)
        {
            var min = 100d;
            var max = -100d;
            foreach (var hour in Enum.GetValues<InfoClimatManager.Hour>())
            {
                if (infoClimatManager.GetForecast(ForecastDate, hour)?.Temperature?.At2m - 273.15 < min)
                {
                    min = infoClimatManager.GetForecast(ForecastDate, hour)?.Temperature?.At2m - 273.15 ?? 0;
                }
                if (infoClimatManager.GetForecast(ForecastDate, hour)?.Temperature?.At2m - 273.15 > max)
                {
                    max = infoClimatManager.GetForecast(ForecastDate, hour)?.Temperature?.At2m - 273.15 ?? 0;
                }
            }
            MinTemp = min;
            MaxTemp = max;
        }

    }
}
