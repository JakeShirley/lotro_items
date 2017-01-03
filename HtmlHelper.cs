using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lotro_items
{
    public static class HtmlHelper
    {
        public static async Task<HtmlNode> FindChildAttributesContains(this HtmlNode root, string attributeKey, string attributeValue, bool ignoreValueCase = false)
        {
            string baseString = ignoreValueCase ? attributeValue.ToLower() : attributeValue;

            var childMatch = root
               .Descendants()
               .Where(d =>
                   !d.Name.Contains("#text") &&
                   d.Attributes.Contains(attributeKey)
                   && d.Attributes[attributeKey].Value.Contains(attributeValue, ignoreValueCase)
                );

            int matchCount = childMatch.Count();
            Debug.Assert(matchCount == 1 || matchCount == 0, "Not a good search");

            if (matchCount >= 1)
            {
                return childMatch.First();
            }

            return null;
        }

        public static async Task<HtmlNode> FindChildTitleContains(this HtmlNode root, string childTitle, bool ignoreCase = false)
        {
            return await FindChildAttributesContains(root, "title", childTitle, ignoreCase);
        }

        public static async Task<HtmlNode> FindChildWithType(this HtmlNode root, string childType)
        {
            var childMatch = root
               .Descendants(childType)
               .Where(d =>
                   d.Name == childType
                );

            int matchCount = childMatch.Count();
            Debug.Assert(matchCount == 1 || matchCount == 0, "Not a good search");

            if (matchCount >= 1)
            {
                return childMatch.First();
            }

            return null;
        }

        public static async Task<HtmlNode> FindChildClassContains(this HtmlNode root, string childClass, bool ignoreCase = false)
        {
            return await FindChildAttributesContains(root, "class", childClass, ignoreCase);
        }

        public static async Task<HtmlNode> FindChildStyleContains(this HtmlNode root, string childStyle, bool ignoreCase = false)
        {
            return await FindChildAttributesContains(root, "style", childStyle, ignoreCase);
        }

        public static async Task<HtmlNode> FindChildTextContains(this HtmlNode root, string containedText, bool ignoreCase = false)
        {
            var childMatch = root
               .Descendants()
               .Where(d =>
                   !d.Name.Contains("#text") && d.InnerText.Contains(containedText, ignoreCase)
           );

            int matchCount = childMatch.Count();
            Debug.Assert(matchCount == 1 || matchCount == 0, "Not a good search");

            if (matchCount >= 1)
            {
                return childMatch.First();
            }

            return null;
        }
    }
}
