using System.Globalization;

namespace AppCommon.Extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// Returns the name of the day of the week for a DateOnly
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="capitalizeFirstLetter">Indicates if the day should start with a capital letter (default: true)</param>
        /// <returns>The name of the day</returns>
        public static string GetDayOfWeek(this DateOnly date, CultureInfo cultureInfo, bool capitalizeFirstLetter = true)
        {
            // Convert DateOnly to DateTime to access cultureInfo formatting methods
            string day = date.ToDateTime(TimeOnly.MinValue).ToString("dddd", cultureInfo);
            // Apply capitalization according to parameter
            if (capitalizeFirstLetter && day.Length > 0)
            {
                return char.ToUpper(day[0]) + day.Substring(1);
            }
            else
            {
                return day;
            }
        }

        /// <summary>
        /// Returns the abbreviated name of the day of the week (3 characters)
        /// </summary>
        /// <param name="date">The date</param>
        /// <returns>The abbreviated name of the day</returns>
        public static string GetShortDayOfWeek(this DateOnly date, CultureInfo cultureInfo)
        {
            return date.ToDateTime(TimeOnly.MinValue).ToString("ddd", cultureInfo);
        }
    }
}
