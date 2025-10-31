using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Extensions;

public static class AppExtensions
{

    /// <summary>
    /// a better ToString that works for many different types
    /// </summary>
    /// <param name="entity">the object to display as a string</param>
    /// <returns>the object as a string</returns>
    public static string AsString(this object obj, int maxDepth = 3)
    {
        return FormatObject(obj, 0, maxDepth, new HashSet<object>(), "");
    }

    private static string FormatObject(object obj, int currentDepth, int maxDepth, HashSet<object> visited, string indent)
    {
        if (obj == null)
            return "null";

        if (visited.Contains(obj))
            return "[Circular Reference]";

        // Handle simple types
        if (obj is string || obj.GetType().IsPrimitive || obj is decimal || obj is DateTime || obj is TimeSpan || obj is Guid)
            return obj.ToString()!;

        // Prevent infinite recursion
        if (currentDepth >= maxDepth)
            return "...";

        Type type = obj.GetType();
        var sb = new StringBuilder();

        // Handle IEnumerable (arrays, lists, etc.)
        if (obj is IEnumerable enumerable && !(obj is string))
        {
            sb.AppendLine($"{type.Name} [");
            visited.Add(obj);
            foreach (var item in enumerable)
            {
                sb.AppendLine($"{indent}  - {FormatObject(item, currentDepth + 1, maxDepth, visited, indent + "  ")}");
            }
            visited.Remove(obj);
            sb.Append($"{indent}]");
        }
        else
        {
            sb.AppendLine($"{type.Name} {{");
            visited.Add(obj);
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                object? value;
                try
                {
                    value = prop.GetValue(obj, null);
                }
                catch
                {
                    value = "[unreadable]";
                }

                sb.AppendLine($"{indent}  {prop.Name}: {FormatObject(value, currentDepth + 1, maxDepth, visited, indent + "  ")}");
            }
            visited.Remove(obj);
            sb.Append($"{indent}}}");
        }

        return sb.ToString();

    }
}
