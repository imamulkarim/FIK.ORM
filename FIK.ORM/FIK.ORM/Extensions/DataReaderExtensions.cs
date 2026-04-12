using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FIK.ORM.Extensions
{
    internal static class DataReaderExtensions
    {
        public static T GetValueOrDefault<T>(this IDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return default(T);

            object value = reader.GetValue(ordinal);

            if (value is T typedValue)
                return typedValue;

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object converted = Convert.ChangeType(value, targetType);

            return (T)converted;
        }
    }
}
