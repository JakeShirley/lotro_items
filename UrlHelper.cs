using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lotro_items
{
    public static class UrlHelper
    {
        public static string GetDomain(string url)
        {
            Regex urlRegex = new Regex("^(?:https?:\\/\\/)?(?:[^@\\/\n]+@)?(?:www\\.)?([^:\\/\\n]+)");
            var urlMatch = urlRegex.Match(url);
            if (urlMatch.Groups.Count > 0)
            {
                return urlMatch.Groups[0].Value;
            }

            return "";
        }
    }
       
}
