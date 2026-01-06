using System.Windows.Media;

namespace AppCommon.Helpers
{
    public static class ColorHelper
    {
        /// <summary>
        /// Returns a color interpolated from a predefined gradient based on the specified temperature.
        /// The method normalizes the temperature within the range defined by <paramref name="minTemp"/> and <paramref name="maxTemp"/>,
        /// then selects and interpolates between key colors to represent the temperature visually.
        /// </summary>
        /// <param name="temperature">The temperature value to map to a color.</param>
        /// <param name="minTemp">The minimum temperature of the range.</param>
        /// <param name="maxTemp">The maximum temperature of the range.</param>
        /// <returns>
        /// A <see cref="Color"/> corresponding to the temperature, interpolated between dark blue, light blue, light pink, pink, red, and purple.
        /// </returns>
        public static Color GetColorMultipoint(double temperature, double minTemp, double maxTemp)
        {
            // Normalize the temperature within the specified range.
            // The null check above ensures 'temperature' is not null here, so direct conversion is safe.
            var normalized = Math.Clamp(((double)temperature - minTemp) / (maxTemp - minTemp), 0.0, 1.0);

            // Define key colors
            Color[] colors =
            {
                Color.FromRgb(0, 0, 255),       // Dark blue
                Color.FromRgb(100, 150, 255),   // Light blue
                Color.FromRgb(255, 150, 200),   // Light pink
                Color.FromRgb(255, 100, 150),   // Pink
                Color.FromRgb(255, 0, 0),       // Red
                Color.FromRgb(150, 0, 200)      // Purple/Violet
            };

            // Calculate in which segment we are
            var scaledValue = normalized * (colors.Length - 1);
            var index = (int)scaledValue;
            var fraction = scaledValue - index;

            // Handle edge case
            if (index >= colors.Length - 1)
            {
                return colors[^1];
            }

            // Interpolate between two adjacent colors
            return InterpolateColor(colors[index], colors[index + 1], fraction);
        }

        /// <summary>
        /// Interpolates between two colors based on the specified fraction.
        /// This method calculates the intermediate color by linearly interpolating
        /// each RGB component between <paramref name="c1"/> and <paramref name="c2"/>,
        /// according to the <paramref name="fraction"/> value.
        /// </summary>
        /// <param name="c1">The starting color for interpolation.</param>
        /// <param name="c2">The ending color for interpolation.</param>
        /// <param name="fraction">
        /// A value between 0.0 and 1.0 representing the interpolation factor,
        /// where 0.0 returns <paramref name="c1"/> and 1.0 returns <paramref name="c2"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Color"/> instance representing the interpolated color.
        /// </returns>
        private static Color InterpolateColor(Color c1, Color c2, double fraction)
        {
            int r = (int)(c1.R + (c2.R - c1.R) * fraction);
            int g = (int)(c1.G + (c2.G - c1.G) * fraction);
            int b = (int)(c1.B + (c2.B - c1.B) * fraction);

            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }
    }
}
