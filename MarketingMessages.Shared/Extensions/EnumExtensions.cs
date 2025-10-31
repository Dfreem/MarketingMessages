using MarketingMessages.Shared.Enums;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Extensions;

public static class EnumExtensions
{

    public static string UnKabobify(this string kabob)
    {
        string[] bobs = kabob.Split('-');
        string result = "";
        for (int i = 0; i < bobs.Length; i++)
        {
            string bob = bobs[i];
            bob = string.Concat(bob[0].ToString().ToUpper(), bob.AsSpan(1));
            result += bob;
        }
        return result;
    }

    public static string KabobToTitleCase(this string kabob)
    {
        var words = kabob.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length <= 1)
            return words[0] ?? "";
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = $"{char.ToUpper(words[i][0])}{words[i][1..]}";
        }
        return string.Join(" ", words);
    }

    public static string Kabobify<T>(this T value, bool separateNumbers = true) where T : struct, Enum
    {
        string enumName = Enum.GetName(value) ?? "";
        if (string.IsNullOrEmpty(enumName))
            return "";
        return enumName.Kabobify(separateNumbers);

    }

    public static string Kabobify(this string value, bool separateNumbers = true)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            // Add a hyphen before uppercase letters unless part of a capitalized group
            if (i > 0 &&
                (char.IsUpper(c) && !char.IsUpper(value[i - 1]) ||
                 char.IsUpper(c) && i + 1 < value.Length && char.IsLower(value[i + 1]) ||
                 separateNumbers && char.IsDigit(c) && !char.IsDigit(value[i - 1])))
            {
                builder.Append('-');
            }

            builder.Append(char.ToLower(c));
        }
        return builder.ToString();
    }
}

public static class StateAbbreviationExtensions
{
    public static StateAbbreviation ToStateAbbreviation(this string stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName))
            return StateAbbreviation.None;

        stateName = stateName.Trim();

        foreach (var field in typeof(StateAbbreviation).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null &&
                string.Equals(descriptionAttr.Description, stateName, StringComparison.OrdinalIgnoreCase))
            {
                return (StateAbbreviation)field.GetValue(null)!;
            }
        }

        // Optionally: try parsing directly if user gave abbreviation ("OR" or "bc")
        if (Enum.TryParse<StateAbbreviation>(stateName, true, out var parsed))
            return parsed;

        return StateAbbreviation.None;
    }
    /// <summary>
    /// Convert a StateAbbreviation enum value to its full name (Description) or fallback to enum name.
    /// </summary>
    public static string ToFullName(this StateAbbreviation abbreviation)
    {
        var field = typeof(StateAbbreviation).GetField(abbreviation.ToString());
        if (field != null)
        {
            var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null && !string.IsNullOrWhiteSpace(descriptionAttr.Description))
            {
                return descriptionAttr.Description;
            }
        }

        return abbreviation.ToString(); // fallback
    }
}

