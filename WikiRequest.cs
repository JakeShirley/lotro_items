using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lotro_items
{
    public static class ObjectExtensions
    {
        public static string PropertyList(this object obj)
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
            }
            return sb.ToString();
        }
    }

    public static class StringExtensions
    {
        public static bool Contains(this string str, string other, bool isCaseInsensitive)
        {
            if(isCaseInsensitive)
            {
                return str.ToLower().Contains(other.ToLower());
            }
            else {
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

    public static class EnumParser
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
                if (Enum.IsDefined(typeof(TEnum), enumValue)) {
                    return enumValue;
                }
            }
            catch (ArgumentException)
            { }

            Debug.Fail("Invalid enum value");
            return default(TEnum);
        }
    }

    struct CurrencyAmmount
    {
        public int Gold { get; set; }
        public int Silver { get; set; }
        public int Copper { get; set; }
    }

    enum ItemBindingType
    {
        BindToAccountOnAcquire,
        BindOnEquip
    }

    enum ItemCategory
    {
        // Armor
        Chest,
        Cloak,
        Feet,
        Gloves,
        Head,
        Legs,
        Shield,
        Shoulder,

        HeavyArmour,
        HeavyShield,

        LightArmour,

        MediumArmour,


        // Jewelery
        Ear,
        Finger,
        Neck,
        Pocket,
        Wrist
    }

    enum ItemDurabilityType
    {
        Normal,
        Tough
    }

    enum ItemAttributeType
    {
        Will,
        Fate,
        CriticalRaiting,
        MaximumMorale,
        Vitality,
        Agility,
        CriticalRating
    }
    
    class ItemDurabilityInfo
    {
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public ItemDurabilityType Type;
    }


    class ItemStats
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconURL { get; set; }
        public int ItemLevel { get; set; }
        public ItemCategory Category { get; set; }
        public ItemBindingType BindingType { get; set; }
        public ItemDurabilityInfo Durability { get; set; } = new ItemDurabilityInfo();
        public int MinimumLevel { get; set; }
        public CurrencyAmmount Worth { get; set; } = new CurrencyAmmount();
        public Dictionary<ItemAttributeType, int> Attributes { get; set; } = new Dictionary<ItemAttributeType, int>();

        private static bool _tryParseItemLevel(string value, ref ItemStats itemStats)
        {
            Regex itemLevelRegex = new Regex("Item Level: ([0-9]+)");
            var itemLevelMatch = itemLevelRegex.Matches(value);
            if (itemLevelMatch.Count == 1)
            {
                itemStats.ItemLevel = int.Parse(itemLevelMatch[0].Groups[1].Value);
                return true;
            }
            
            return false;
        }

        private static bool _tryParseAttribute(string value, ref ItemStats itemStats)
        {
            Regex attributeRegex = new Regex("([+-]?[0-9]+)\\s+([a-zA-Z ]+)");
            var attributeRegexMatch = attributeRegex.Matches(value);
            if (attributeRegexMatch.Count == 1)
            {
               int arrtibuteLevel = int.Parse(attributeRegexMatch[0].Groups[1].Value);
                ItemAttributeType attributeType = EnumParser.TryFromString<ItemAttributeType>(attributeRegexMatch[0].Groups[2].Value);

                itemStats.Attributes.Add(attributeType, arrtibuteLevel);

                return true;
            }

            return false;
        }

        private static bool _tryParseMinumumLevel(string value, ref ItemStats itemStats)
        {
            Regex levelRegex = new Regex("Minimum\\s+Level\\s+([0-9]+)");
            var levelRegexMatch = levelRegex.Matches(value);
            if (levelRegexMatch.Count == 1)
            {
                itemStats.MinimumLevel = int.Parse(levelRegexMatch[0].Groups[1].Value);

                return true;
            }

            return false;
        }

        private static bool _tryParseDurabilityLevel(string value, ref ItemStats itemStats)
        {
            Regex durabilityRegex = new Regex("([0-9]+)\\s*?\\/\\s*?([0-9]+)");
            var durabilityMatch = durabilityRegex.Matches(value);
            if (durabilityMatch.Count == 1)
            {
                int currentLevel = int.Parse(durabilityMatch[0].Groups[1].Value);
                int maxLevel = int.Parse(durabilityMatch[0].Groups[2].Value);

                if(itemStats.Durability == null)
                {
                    itemStats.Durability = new ItemDurabilityInfo();
                }

                itemStats.Durability.CurrentLevel = currentLevel;
                itemStats.Durability.MaxLevel = maxLevel;

                return true;
            }

            return false;
        }

        private static bool _tryParseWorth(string value, ref ItemStats itemStats)
        {
            string trimmedValue = value.HtmlTrim();
            Regex worthRegex = new Regex("Worth:\\s+?([0-9]+)(\\s+?([0-9]+)(\\s+?([0-9]+))?)?");
            var worthMatch = worthRegex.Matches(trimmedValue);
            if (worthMatch.Count == 1)
            {
                var worthGroups = worthMatch[0].Groups;

                int gold = 0, silver = 0, copper = 0;

                if(worthGroups[3].Value.Empty()) // Copper
                {
                    copper = int.Parse(worthGroups[1].Value);
                }
                else if(worthGroups[5].Value.Empty()) // Silver, copper
                {
                    silver = int.Parse(worthGroups[1].Value);
                    copper = int.Parse(worthGroups[3].Value);
                }
                else // Gold?, silver, copper
                {
                    gold = int.Parse(worthGroups[1].Value);
                    silver = int.Parse(worthGroups[3].Value);
                    copper = int.Parse(worthGroups[5].Value);
                }

                itemStats.Worth = new CurrencyAmmount()
                {
                    Copper = copper,
                    Silver = silver,
                    Gold = gold,
                };

                return true;
            }

            return false;
        }

        public static async Task<ItemStats> FromWebPage(string bodyContent)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.LoadHtml(bodyContent);

            HtmlNode rootNode = null;

            await htmlDoc.DocumentNode.FindChildClassContains("tooltip-content itemtooltip", true).ContinueWith(
                rootQuery =>
            {
                rootNode = rootQuery.Result;
            });
           

            if(rootNode != null)
            {
                ItemStats result = new ItemStats();


                await rootNode.FindChildClassContains("image").ContinueWith(imageQuery =>
                {
                    if(imageQuery.Result != null)
                    {
                        result.IconURL = imageQuery.Result.FirstChild.Attributes["src"].Value;
                    }
                });
                

                var titleElements = rootNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.StartsWith("qc-"));
                if(titleElements.Count() == 1)
                {
                    result.Name = titleElements.First().FirstChild.InnerText.Trim();
                }

                var detailElementsElem = rootNode.Descendants("ul");
                if(detailElementsElem.Count() == 1)
                {
                    var detailElements = detailElementsElem.First();

                    // Binding Type
                    await detailElements.FindChildTitleContains("Bind On", true).ContinueWith(bindResult =>
                    {
                        Debug.Assert(bindResult.Result != null, "No binding field found");
                        if (bindResult.Result != null)
                        {
                            result.BindingType = EnumParser.TryFromString<ItemBindingType>(bindResult.Result.InnerText);
                        }
                    });

                    // Item Level
                    await detailElements.FindChildTextContains("Item Level", true).ContinueWith(levelResult =>
                    {
                        if(levelResult.Result != null)
                        {
                            bool parsedItemLevel = _tryParseItemLevel(levelResult.Result.InnerText, ref result);
                            Debug.Assert(parsedItemLevel, "Found item level but could not parse it");
                        }
                    });

                    // Item Category
                    await detailElements.FindChildTitleContains("Category:", true).ContinueWith(categoryResult =>
                    {
                        Debug.Assert(categoryResult.Result != null, "No category field found");
                        if (categoryResult.Result != null)
                        {
                            result.Category = EnumParser.TryFromString<ItemCategory>(categoryResult.Result.InnerText);
                        }
                    });

                    // Attributes
                    await detailElements.FindChildClassContains("attrib", true).ContinueWith(attributesResult =>
                    {
                        if (attributesResult.Result != null)
                        {
                           foreach(var child in attributesResult.Result.ChildNodes)
                            {
                                if(child.Name != "br" && child.Name != "div" && !child.InnerText.IsWhiteSpace())
                                {
                                    bool parsedAttribute = _tryParseAttribute(child.InnerText, ref result);
                                    Debug.Assert(parsedAttribute, "Could not parse item attribute");
                                }
                            }
                        }
                    });

                    // Durability Type
                    await detailElements.FindChildStyleContains("float: right", true).ContinueWith(durabilityTypeQuery =>
                    {
                        if (durabilityTypeQuery.Result != null)
                        {
                            result.Durability.Type = EnumParser.TryFromString<ItemDurabilityType>(durabilityTypeQuery.Result.InnerText);
                        }
                    });

                    // Durability Level
                    await detailElements.FindChildTitleContains("Durability", true).ContinueWith(durabilityLevelQuery =>
                    {
                        if (durabilityLevelQuery.Result != null)
                        {
                            bool parsedDurabilityLevel = _tryParseDurabilityLevel(durabilityLevelQuery.Result.NextSibling.InnerText, ref result);
                            Debug.Assert(parsedDurabilityLevel, "Could not parse durability level");
                        }
                    });

                    // Minimum Level
                    await detailElements.FindChildTextContains("Minimum Level", true).ContinueWith(levelQuery =>
                    {
                        if (levelQuery.Result != null)
                        {
                            bool parsedMinLevel = _tryParseMinumumLevel(levelQuery.Result.InnerText.HtmlTrim(), ref result);
                            Debug.Assert(parsedMinLevel, "Could not parse minimum level");
                        }
                    });

                    // Description
                    await detailElements.FindChildClassContains("flavor", true).ContinueWith(descriptionQuery =>
                    {
                        if (descriptionQuery.Result != null)
                        {
                            result.Description = descriptionQuery.Result.InnerText.HtmlTrim();
                        }
                    });

                    // Worth
                    await detailElements.FindChildTextContains("Worth:", true).ContinueWith(currencyQuery =>
                    {
                        if (currencyQuery.Result != null)
                        {
                            bool parsedWorth = _tryParseWorth(currencyQuery.Result.InnerText, ref result);
                            Debug.Assert(parsedWorth, "Could not parse worth");
                        }
                    });
                }

                return result;
            }

            return null;
        }
    }

    class WikiRequest
    {
        private string _url;
        public WikiRequest(string url)
        {
            _url = url;
        }

        public async Task<ItemStats> requestItem()
        {
            string body = await sendRequest();
            ItemStats result = await ItemStats.FromWebPage(body);

            if(result != null) {
                Regex urlRegex = new Regex("^(?:https?:\\/\\/)?(?:[^@\\/\n]+@)?(?:www\\.)?([^:\\/\\n]+)");
                var urlMatch = urlRegex.Match(_url);
                if(urlMatch.Groups.Count > 0)
                {
                    result.IconURL = urlMatch.Groups[0].Value + result.IconURL;
                }
            }

            return result;
        }

        public async Task<string> sendRequest()
        {
            //Create an HTTP client object
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();

            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri(_url);

            //Send the GET request asynchronously and retrieve the response as a string.
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            return httpResponseBody;
        }
    }
}
