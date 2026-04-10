using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIK.ORM.Extensions
{
    internal static class StringExtensions
    {
        public static string SanitizeForSql(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Basic sanitization: Escape single quotes (SQL standard)
            return input.Replace("'", "''");
        }

        public static string GetOnlyTableName(this string fullTableName)
        {
            if (fullTableName.Contains("."))
            {
                return fullTableName.Split('.').Last();
            }
            return fullTableName;
        }
    }
}
