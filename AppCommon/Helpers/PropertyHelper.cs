using AppCommon.Attributs;
using System.Reflection;

namespace AppCommon.Helpers
{
    public static class PropertyHelper
    {
        /// <summary>
        /// Retrieves all readable public properties of the specified object and their values.
        /// </summary>
        /// <param name="obj">The object whose properties are to be retrieved.</param>
        /// <returns>
        /// A dictionary containing the names and values of all readable public properties.
        /// If a property cannot be accessed, its value will be an error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided object is null.</exception>
        public static Dictionary<string, object> GetReadableProperties(object obj, Type? attribute = null)
        {
            ArgumentNullException.ThrowIfNull(obj);

            var properties = new Dictionary<string, object>();
            var type = obj.GetType();

            // Retrieve all public properties
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                // Check if the property has a getter and optionally if it has the specified attribute
                if (prop.CanRead && (attribute == null || Attribute.IsDefined(prop, attribute)))
                {
                    try
                    {
                        // Retrieve the value
                        var value = prop.GetValue(obj);
                        if (attribute == typeof(LabelAttribute))
                        {
                            var att = prop.GetCustomAttribute<LabelAttribute>();
                            properties.Add(att?.Text ?? prop.Name, value ?? "null");
                        }
                        else
                        {
                            properties.Add(prop.Name, value ?? "null");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle potential errors when accessing certain properties
                        properties.Add(prop.Name, $"Error: {ex.Message}");
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// Retrieves all readable public properties of the specified object, filtered by the specified type.
        /// </summary>
        /// <typeparam name="T">The type to filter the properties by.</typeparam>
        /// <param name="obj">The object whose properties are to be retrieved.</param>
        /// <returns>
        /// A dictionary containing the names and values of all readable public properties
        /// that match the specified type.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided object is null.</exception>
        public static Dictionary<string, object> GetReadableProperties<T>(object obj)
        {
            var allProperties = GetReadableProperties(obj);
            var filteredProperties = new Dictionary<string, object>();

            foreach (var prop in allProperties)
            {
                if (prop.Value is T)
                {
                    filteredProperties.Add(prop.Key, prop.Value);
                }
            }

            return filteredProperties;
        }
    }
}
