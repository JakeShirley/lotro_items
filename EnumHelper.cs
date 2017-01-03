using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lotro_items
{
    public static class EnumHelper
    {
        public static TEnum TryFromString<TEnum>(string str) where TEnum : struct
        {
            string parsedString = str;
            // Remove white space
            parsedString = Regex.Replace(parsedString, @"\s+", "");
            parsedString = parsedString.ToLower();

            try
            {
                TEnum enumValue = (TEnum)Enum.Parse(typeof(TEnum), parsedString, true);
                if (Enum.IsDefined(typeof(TEnum), enumValue))
                {
                    return enumValue;
                }
            }
            catch (ArgumentException)
            { }

            Debug.Fail("Invalid enum value");
            return default(TEnum);
        }
    }
}
