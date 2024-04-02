using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;

namespace plugin.commands;

public class GangDebugCmd(ICS2Gangs gangs) : Command(gangs)
{
    public override void OnCommand(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_execute_in_game");
            return;
        }
        if (!executor.IsReal())
            return;
        var steam = executor.AuthorizedSteamID;
        if (steam == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Try again in a few seconds.");
            return;
        }
        if (!AdminManager.PlayerHasPermissions(executor, gangs.Config.DebugPermission!))
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error", "You do not have permission to use this command.");
            return;
        }

        GangPlayer? playerInfo = gangs.GetGangsService().GetGangPlayer(steam.SteamId64).GetAwaiter()
            .GetResult();

        if (playerInfo == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You were not found in the database. Try again in a few seconds.");
            return;
        }

        Server.PrintToConsole($"---PLAYER DEBUG---: {playerInfo}");

        if(playerInfo.GangId != null) {
            Server.PrintToConsole($"---GANG DEBUG---: {gangs.GetGangsService().GetGang(playerInfo.GangId.Value).GetAwaiter().GetResult()}");
        } else {
            Server.PrintToConsole("---GANG DEBUG---: Player is not in a gang.");
        }

    }
}