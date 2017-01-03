using HtmlAgilityPack;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lotro_items
{
    struct CurrencyAmmount
    {
        public int Gold { get; set; }
        public int Silver { get; set; }
        public int Copper { get; set; }

        public override string ToString()
        {
            string result = "";

            if(Gold != 0)
            {
                result += string.Format("{0} Gold ", Gold);
            }
            if (Silver != 0)
            {
                result += string.Format("{0} Silver ", Silver);
            }
            if (Copper != 0)
            {
                result += string.Format("{0} Copper ", Copper);
            }

            if(result.Empty())
            {
                return "None";
            }

            return result.Trim();
        }
    }

    enum ItemBindingType
    {
        Invalid,

        BindToAccountOnAcquire,
        BindToAccount,
        BindOnEquip,
        BindOnAcquire
    }

    enum ItemCategory
    {
        Invalid,

        // Armor
        Chest,
        Cloak,
        Feet,
        Gloves,
        Head,
        Legs,
        Shield,
        Shoulder,

        // Weapons
        Dagger,

        HeavyArmour,
        HeavyShield,

        LightArmour,

        MediumArmour,

        // Misc
        Ingredient,


        // Jewelery
        Ear,
        Finger,
        Neck,
        Pocket,
        Wrist
    }

    enum ItemDurabilityType
    {
        Invalid,

        Normal,
        Tough
    }

    enum ItemAttributeType
    {
        Invalid,

        Will,
        Fate,
        CriticalRaiting,
        MaximumMorale,
        Vitality,
        Agility,
        CriticalRating,
        MaximumPower,
        EvadeRating,
        PhysicalMasteryRating,
        TacticalMasteryRating,
        CriticalDefense,
        InCombatMoraleRegen,
        CriticalDefence,
        FinesseRating,
        FireMitigation,
        Might,
        TacticalMitigation,
        TacticalCriticalMultiplier,
        ShadowDefence,
        OutOfCombatRunSpeed,
        AutoAttackCriticalHitChance,
        ParryRating,
        ResistanceRating,
        PhysicalMitigation,

        // Typos
        MaximunMorale = MaximumMorale,
        MaximunPower = MaximumPower,
    }

    public enum ItemAttributeBuffType
    {
        Invalid,

        Absolute,
        Percent,
    }

    struct ItemAttributeBuff
    {
        public ItemAttributeBuffType Type { get; set; }
        public int Value { get; set; }
    }

    class ItemDurabilityInfo
    {
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public ItemDurabilityType Type;

        public override string ToString()
        {
            return string.Format("{0}/{1} - {2}", CurrentLevel, MaxLevel, Type);
        }
    }


    class ItemDescription
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconURL { get; set; } = "";
        public int ItemLevel { get; set; }
        public ItemCategory Category { get; set; }
        public ItemBindingType BindingType { get; set; }
        public ItemDurabilityInfo Durability { get; set; } = new ItemDurabilityInfo();
        public int MinimumLevel { get; set; }
        public CurrencyAmmount Worth { get; set; } = new CurrencyAmmount();
        public Dictionary<ItemAttributeType, ItemAttributeBuff> Attributes { get; set; } = new Dictionary<ItemAttributeType, ItemAttributeBuff>();
        public int Armor { get; set; }
        public float DPS { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("Name: " + Name);
            sb.AppendLine("Description: " + (Description.Empty() ? "None" : Description));
            sb.AppendLine("Level: " + ItemLevel);
            sb.AppendLine("Category: " + Category.ToString());
            sb.AppendLine("Binding Type: " + BindingType.ToString());
            sb.AppendLine("Durability: " + Durability.ToString());
            sb.AppendLine("Minimum Level: " + MinimumLevel);
            sb.AppendLine("Worth: " + Worth.ToString());
            sb.AppendLine("Armour: " + Armor.ToString());
            sb.AppendLine("Attributes: " + (Attributes.Count == 0 ? "None" : ""));
            foreach(var attribute in Attributes)
            {
                sb.AppendLine(string.Format(" {0}{1}{2} {3}", (attribute.Value.Value < 0 ? "-" : "+"), attribute.Value.ToString(), attribute.Value.Type == ItemAttributeBuffType.Percent ? "%" : "", attribute.Key));
            }

            return sb.ToString();
        }

        private static bool _tryParseItemLevel(string value, ref ItemDescription itemStats)
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

        private static bool _tryParseAttribute(string value, ref ItemDescription itemStats)
        {
            Regex attributeRegex = new Regex("([+-]?[0-9]+)\\s?%?\\s+([a-zA-Z -]+)");
            var attributeRegexMatch = attributeRegex.Matches(value);
            if (attributeRegexMatch.Count == 1)
            {
                int arrtibuteLevel = int.Parse(attributeRegexMatch[0].Groups[1].Value);
                ItemAttributeType attributeType = EnumHelper.TryFromString<ItemAttributeType>(attributeRegexMatch[0].Groups[2].Value);
                ItemAttributeBuffType buffType = value.Contains("%") ? ItemAttributeBuffType.Percent : ItemAttributeBuffType.Absolute;

                itemStats.Attributes.Add(attributeType, new ItemAttributeBuff() { Value = arrtibuteLevel, Type = buffType });

                return true;
            }

            return false;
        }

        private static bool _tryParseMinumumLevel(string value, ref ItemDescription itemStats)
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

        private static bool _tryParseDurabilityLevel(string value, ref ItemDescription itemStats)
        {
            Regex durabilityRegex = new Regex("([0-9]+)\\s*?\\/\\s*?([0-9]+)");
            var durabilityMatch = durabilityRegex.Matches(value);
            if (durabilityMatch.Count == 1)
            {
                int currentLevel = int.Parse(durabilityMatch[0].Groups[1].Value);
                int maxLevel = int.Parse(durabilityMatch[0].Groups[2].Value);

                if (itemStats.Durability == null)
                {
                    itemStats.Durability = new ItemDurabilityInfo();
                }

                itemStats.Durability.CurrentLevel = currentLevel;
                itemStats.Durability.MaxLevel = maxLevel;

                return true;
            }

            return false;
        }

        private static bool _tryParseArmourLevel(string value, ref ItemDescription itemStats)
        {
            Regex armourRegex = new Regex("([0-9]+)\\s+Armour");
            var armourValueMatch = armourRegex.Matches(value);
            if (armourValueMatch.Count == 1)
            {
                itemStats.Armor = int.Parse(armourValueMatch[0].Groups[1].Value);

                return true;
            }

            return false;
        }

        private static bool _tryParseDPS(string value, ref ItemDescription itemStats)
        {
            Regex dpsRegex = new Regex("([0-9]+(\\.[0-9]+)?)\\s+[dD][pP][sS]");
            var dpsMatch = dpsRegex.Matches(value);
            if (dpsMatch.Count == 1)
            {
                itemStats.DPS = float.Parse(dpsMatch[0].Groups[1].Value);

                return true;
            }

            return false;
        }

        private static bool _tryParseWorth(string value, ref ItemDescription itemStats)
        {
            string trimmedValue = value.HtmlTrim();
            Regex worthRegex = new Regex("Worth:\\s+?([0-9]+)(\\s+?([0-9]+)(\\s+?([0-9]+))?)?");
            var worthMatch = worthRegex.Matches(trimmedValue);
            if (worthMatch.Count == 1)
            {
                var worthGroups = worthMatch[0].Groups;

                int gold = 0, silver = 0, copper = 0;

                if (worthGroups[3].Value.Empty()) // Copper
                {
                    copper = int.Parse(worthGroups[1].Value);
                }
                else if (worthGroups[5].Value.Empty()) // Silver, copper
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

        public static async Task<ItemDescription> FromWebPage(string bodyContent)
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


            if (rootNode != null)
            {
                ItemDescription result = new ItemDescription();


                result.IconURL = rootNode.FirstChild.FirstChild.FirstChild.FirstChild.Attributes["src"].Value;


                var titleElements = rootNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.StartsWith("qc-"));
                if (titleElements.Count() == 1)
                {
                    result.Name = titleElements.First().FirstChild.InnerText.Trim();
                }

                var detailElementsElem = rootNode.Descendants("ul");
                if (detailElementsElem.Count() == 1)
                {
                    var detailElements = detailElementsElem.First();

                    // Binding Type
                    await detailElements.FindChildTitleContains("Bind", false).ContinueWith(bindResult =>
                    {
                        if (bindResult.Result != null)
                        {
                            result.BindingType = EnumHelper.TryFromString<ItemBindingType>(bindResult.Result.InnerText);
                        }
                    });

                    // Item Level
                    await detailElements.FindChildTextContains("Item Level", false).ContinueWith(levelResult =>
                    {
                        if (levelResult.Result != null)
                        {
                            bool parsedItemLevel = _tryParseItemLevel(levelResult.Result.InnerText, ref result);
                            Debug.Assert(parsedItemLevel, "Found item level but could not parse it");
                        }
                    });

                    // Item Category
                    await detailElements.FindChildTitleContains("Category:", true).ContinueWith(categoryResult =>
                    {
                        if (categoryResult.Result != null)
                        {
                            result.Category = EnumHelper.TryFromString<ItemCategory>(categoryResult.Result.InnerText);
                        }

                    });

                    // Armour Value
                    await detailElements.FindChildClassContains("khaki", true).ContinueWith(armourValueQuery =>
                    {
                        if (armourValueQuery.Result != null)
                        {
                            if(! _tryParseArmourLevel(armourValueQuery.Result.InnerText, ref result))
                            {
                                if(!_tryParseDPS(armourValueQuery.Result.InnerText, ref result))
                                {
                                    Debug.Fail("Could not parse armor/DPS");
                                }
                            }
                        }
                    });

                    // Attributes
                    await detailElements.FindChildClassContains("attrib", true).ContinueWith(attributesResult =>
                    {
                        if (attributesResult.Result != null)
                        {
                            foreach (var child in attributesResult.Result.ChildNodes)
                            {
                                if (child.Name != "br" && child.Name != "div" && child.Name != "a" && !child.InnerText.IsWhiteSpace())
                                {
                                    string attributeText = child.InnerText;
                                    if(child.NextSibling.Name == "a")
                                    {
                                        attributeText += child.NextSibling.InnerText;
                                    }

                                    if (!attributeText.Contains(":"))
                                    {
                                        bool parsedAttribute = _tryParseAttribute(attributeText, ref result);
                                        Debug.Assert(parsedAttribute, "Could not parse item attribute");
                                    }
                                }
                            }
                        }
                    });

                    // Durability Type
                    await detailElements.FindChildTitleContains("Durability", true).ContinueWith(durabilityTypeQuery =>
                    {
                        if (durabilityTypeQuery.Result != null)
                        {
                            result.Durability.Type = EnumHelper.TryFromString<ItemDurabilityType>(durabilityTypeQuery.Result.ParentNode.PreviousSibling.FirstChild.InnerText);
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
}
