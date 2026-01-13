using System;
using System.Collections.Generic;
using System.Text;

namespace AppCommon.Helpers
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Returns the later of two <see cref="DateTime"/> values.
        /// </summary>
        /// <param name="date1">The first date to compare.</param>
        /// <param name="date2">The second date to compare.</param>
        /// <returns>
        /// The <see cref="DateTime"/> value that is later. If both dates are equal, returns <paramref name="date1"/>.
        /// </returns>
        public static DateTime Max(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }
    }
}
