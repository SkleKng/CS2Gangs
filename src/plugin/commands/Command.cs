using api.plugin;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using plugin.commands.native;
using plugin.extensions;

namespace plugin.commands;

public abstract class Command(ICS2Gangs gangs)
{
    protected readonly ICS2Gangs gangs = gangs;

    public string? Description => null;

    public abstract void OnCommand(CCSPlayerController? executor, CommandInfo info);

    protected TargetResult? GetTarget(CommandInfo command, int argIndex = 1,
        Func<CCSPlayerController, bool>? predicate = null)
    {
        var matches = new NativeTargetModified(command.GetArg(argIndex)).GetTarget(command.CallingPlayer);

        matches.Players = matches.Players
            .Where(player => player.IsValid && player.Connected == PlayerConnectedState.PlayerConnected).ToList();
        if (predicate != null)
            matches.Players = matches.Players.Where(predicate).ToList();

        if (!matches.Any())
        {
            command.ReplyLocalized(gangs.GetBase().Localizer, "player_not_found", command.GetArg(argIndex));
            return null;
        }

        if (matches.Count() > 1 && command.GetArg(argIndex).StartsWith('@'))
            return matches;

        if (matches.Count() == 1 || !command.GetArg(argIndex).StartsWith('@'))
            return matches;

        command.ReplyLocalized(gangs.GetBase().Localizer, "player_found_multiple", command.GetArg(argIndex));
        return null;
    }

    protected internal TargetResult? GetVulnerableTarget(CommandInfo command, int argIndex = 1,
        Func<CCSPlayerController, bool>? predicate = null)
    {
        return GetTarget(command, argIndex,
            p => command.CallingPlayer == null ||
                 (command.CallingPlayer.CanTarget(p) && (predicate == null || predicate(p))));
    }

    protected internal TargetResult? GetSingleTarget(CommandInfo command, int argIndex = 1)
    {
        var matches = command.GetArgTargetResult(argIndex);

        if (!matches.Any())
        {
            command.ReplyLocalized(gangs.GetBase().Localizer, "player_not_found", command.GetArg(argIndex));
            return null;
        }

        if (matches.Count() > 1)
        {
            command.ReplyLocalized(gangs.GetBase().Localizer, "player_found_multiple", command.GetArg(argIndex));
            return null;
        }

        return matches;
    }

    protected string GetTargetLabel(CommandInfo info, int argIndex = 1)
    {
        switch (info.GetArg(argIndex))
        {
            case "@all":
                return "all players";
            case "@bots":
                return "all bots";
            case "@humans":
                return "all humans";
            case "@alive":
                return "alive players";
            case "@dead":
                return "dead players";
            case "@!me":
                return "all except self";
            case "@me":
                return info.CallingPlayer == null ? "Console" : info.CallingPlayer.PlayerName;
            case "@ct":
                return "all CTs";
            case "@t":
                return "all Ts";
            case "@spec":
                return "all spectators";
            default:
                var player = info.GetArgTargetResult(argIndex).FirstOrDefault();
                if (player != null)
                    return player.PlayerName;
                return "unknown";
        }
    }


    protected string GetTargetLabels(CommandInfo info, int argIndex = 1)
    {
        var label = GetTargetLabel(info, argIndex);
        if (label.ToLower().EndsWith("s"))
            return label + "'";
        return label + "'s";
    }
}

public static class CommandExtensions
{
    static bool CanTarget(this CCSPlayerController controller, CCSPlayerController target)
    {
        if (!target.IsReal()) return true;
        return AdminManager.GetPlayerImmunity(controller) > AdminManager.GetPlayerImmunity(target);
    }
}