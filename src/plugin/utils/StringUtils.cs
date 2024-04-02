using CounterStrikeSharp.API.Modules.Utils;

namespace plugin.utils;

internal class StringUtils
{
    internal static string ReplaceChatColors(string message)
    {
        if (message.Contains('{'))
        {
            var modifiedValue = message;
            foreach (var field in typeof(ChatColors).GetFields())
            {
                var pattern = $"{{{field.Name}}}";
                if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)!.ToString(),
                        StringComparison.OrdinalIgnoreCase);
            }

            return modifiedValue;
        }

        return message;
    }

    internal static string RemoveStrings(string message, List<string> stringsToRemove)
    {
        var modifiedValue = message;
        foreach (var s in stringsToRemove)
            modifiedValue = modifiedValue.Replace(s, string.Empty, StringComparison.OrdinalIgnoreCase);
        return modifiedValue;
    }
}