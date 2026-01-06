using System.Reflection;

namespace AppCommon.Extensions
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Copies the properties from the source object to the target object.
        /// Only properties with matching names, types, and accessibility are copied.
        /// </summary>
        /// <param name="source">The source object from which properties are copied.</param>
        /// <param name="target">The target object to which properties are copied.</param>
        public static void CopyPropertiesTo(this object source, object target)
        {
            var sourceProperties = source.GetType().GetProperties();
            var targetProperties = target.GetType().GetProperties();
            PropertyInfo? targetProperty;
            foreach (var sourceProperty in sourceProperties)
            {
                targetProperty = targetProperties.FirstOrDefault(propertyInfo => (propertyInfo.Name ?? "") == (sourceProperty.Name ?? ""));
                if (targetProperty != null && targetProperty.PropertyType.Equals(sourceProperty.PropertyType) && targetProperty.CanWrite && sourceProperty.CanRead && IsPropertyCanCopy(sourceProperty))
                {
                    targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
                }
            }
        }

        /// <summary>
        /// Copies the properties from the source object to the target object.
        /// This is a reverse alias for <see cref="CopyPropertiesTo"/>.
        /// </summary>
        /// <param name="target">The target object to which properties are copied.</param>
        /// <param name="source">The source object from which properties are copied.</param>
        public static void CopyPropertiesFrom(this object target, object source)
        {
            source.CopyPropertiesTo(target);
        }

        /// <summary>
        /// Determines whether a property can be copied based on its type.
        /// </summary>
        /// <param name="prop">The property to check.</param>
        /// <returns>True if the property can be copied; otherwise, false.</returns>
        private static bool IsPropertyCanCopy(PropertyInfo prop)
        {
            var canCopy = prop.PropertyType.Equals(typeof(bool))
                   || prop.PropertyType.Equals(typeof(byte))
                   || prop.PropertyType.Equals(typeof(byte[]))
                   || prop.PropertyType.Equals(typeof(char))
                   || prop.PropertyType.Equals(typeof(DateTime))
                   || prop.PropertyType.Equals(typeof(DBNull))
                   || prop.PropertyType.Equals(typeof(decimal))
                   || prop.PropertyType.Equals(typeof(double))
                   || prop.PropertyType.Equals(typeof(short))
                   || prop.PropertyType.Equals(typeof(int))
                   || prop.PropertyType.Equals(typeof(long))
                   || prop.PropertyType.Equals(typeof(object))
                   || prop.PropertyType.Equals(typeof(sbyte))
                   || prop.PropertyType.Equals(typeof(float))
                   || prop.PropertyType.Equals(typeof(string))
                   || prop.PropertyType.Equals(typeof(ushort))
                   || prop.PropertyType.Equals(typeof(uint))
                   || prop.PropertyType.Equals(typeof(ulong));
            return canCopy;
        }

        /// <summary>
        /// Creates a new instance of type <typeparamref name="T"/> and copies the properties
        /// from the source object to the new instance.
        /// </summary>
        /// <typeparam name="T">The type of the new object to create.</typeparam>
        /// <param name="source">The source object from which properties are copied.</param>
        /// <returns>A new instance of type <typeparamref name="T"/> with copied properties.</returns>
        public static T CloneAs<T>(this object source) where T : new()
        {
            var target = new T();
            source.CopyPropertiesTo(target);
            return target;
        }

        /// <summary>
        /// Converts the source object to an <see cref="int"/>.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns>The <see cref="int"/> representation of the source object.</returns>
        public static int ToInt32(this object source)
        {
            return Convert.ToInt32(source);
        }

        /// <summary>
        /// Converts the source object to a <see cref="long"/>.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns>The <see cref="long"/> representation of the source object.</returns>
        public static long ToLong(this object source)
        {
            return Convert.ToInt64(source);
        }


    }

}