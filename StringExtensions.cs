using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lotro_items
{
    public static class StringExtensions
    {
        public static bool Contains(this string str, string other, bool isCaseInsensitive)
        {
            if (isCaseInsensitive)
            {
                return str.ToLower().Contains(other.ToLower());
            }
            else
            {
                return str.Contains(other);
            }
        }

        public static bool IsWhiteSpace(this string str)
        {
            return str.Length != 0 && str.Trim().Length == 0;
        }

        public static string HtmlTrim(this string str)
        {
            return Regex.Replace(str, "&#160;", "").Trim();
        }

        public static bool Empty(this string str)
        {
            return str.Length == 0;
        }
    }
}
