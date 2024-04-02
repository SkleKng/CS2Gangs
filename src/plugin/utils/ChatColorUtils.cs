using CounterStrikeSharp.API.Modules.Utils;

namespace plugin.utils;

internal class ChatColorUtils
{
    public static readonly char[] AVAILABLE_COLORS =
    {
        ChatColors.White,
        ChatColors.Green,
        ChatColors.Yellow,
        ChatColors.Olive,
        ChatColors.Lime,
        ChatColors.LightPurple,
        ChatColors.Purple,
        ChatColors.Grey,
        ChatColors.Gold,
        ChatColors.Silver,
        ChatColors.Blue,
        ChatColors.DarkBlue
    };

    public static readonly char[] ALL_COLORS =
    {
        ChatColors.White,
        ChatColors.DarkRed,
        ChatColors.Green,
        ChatColors.Yellow,
        ChatColors.Olive,
        ChatColors.Lime,
        ChatColors.Red,
        ChatColors.LightPurple,
        ChatColors.Purple,
        ChatColors.Grey,
        ChatColors.Gold,
        ChatColors.Silver,
        ChatColors.Blue,
        ChatColors.DarkBlue,
        ChatColors.LightRed
    };

    public static bool IsValidColor(char color, bool allColors = false)
    {
        return allColors ? ALL_COLORS.Contains(color) : AVAILABLE_COLORS.Contains(color);
    }

    public static bool IsValidColor(string color, bool allColors = false)
    {
        return allColors ? ALL_COLORS.Contains(StringToColor(color)) : AVAILABLE_COLORS.Contains(StringToColor(color));
    }

    public static string PrettyColorName(char color)
    {
        switch (color)
        {
            case '\u0001':
                return "White";
            case '\u0002':
                return "DarkRed";
            case '\u0004':
                return "Green";
            case '\t':
                return "Yellow";
            case '\u0005':
                return "Olive";
            case '\u0006':
                return "Lime";
            case '\a':
                return "Red";
            case '\u0003':
                return "LightPurple";
            case '\u000e':
                return "Purple";
            case '\b':
                return "Grey";
            case '\u0010':
                return "Gold";
            case '\n':
                return "Silver";
            case '\v':
                return "Blue";
            case '\f':
                return "DarkBlue";
            case '\u000f':
                return "LightRed";
            default:
                return "White";
        }
    }

    public static char StringToColor(string color)
    {
        switch (color.ToLower())
        {
            case "white":
                return ChatColors.White;
            case "darkred":
                return ChatColors.DarkRed;
            case "green":
                return ChatColors.Green;
            case "lightyellow":
                return ChatColors.LightYellow;
            case "lightblue":
                return ChatColors.LightBlue;
            case "olive":
                return ChatColors.Olive;
            case "lime":
                return ChatColors.Lime;
            case "red":
                return ChatColors.Red;
            case "lightpurple":
                return ChatColors.LightPurple;
            case "purple":
                return ChatColors.Purple;
            case "grey":
                return ChatColors.Grey;
            case "yellow":
                return ChatColors.Yellow;
            case "gold":
                return ChatColors.Gold;
            case "silver":
                return ChatColors.Silver;
            case "blue":
                return ChatColors.Blue;
            case "darkblue":
                return ChatColors.DarkBlue;
            case "bluegrey":
                return ChatColors.BlueGrey;
            case "magenta":
                return ChatColors.Magenta;
            case "lightred":
                return ChatColors.LightRed;
            case "orange":
                return ChatColors.Orange;
            default:
                return ChatColors.White;
        }
    }
}