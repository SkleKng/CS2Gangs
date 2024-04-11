using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;

namespace plugin.commands;

public class SetCreditsCmd(ICS2Gangs gangs) : Command(gangs)
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
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Try again in a few seconds.");
            return;
        }
        if (!AdminManager.PlayerHasPermissions(executor, gangs.Config.DebugPermission!))
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "You do not have permission to use this command.");
            return;
        }

        if(info.ArgCount <= 1) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "usage", "css_setcredits <player> (credits)");
            return;
        }


        int credits = 0;

        if (info.ArgCount <= 2)
        {
            if(!int.TryParse(info.GetArg(1), out credits)) {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "Invalid credits amount.");
                return;
            }
            GangPlayer? playerInfo = gangs.GetGangsService().GetGangPlayer(steam.SteamId64).GetAwaiter()
                .GetResult();

            if (playerInfo == null)
            {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                    "You were not found in the database. Try again in a few seconds.");
                return;
            }

            playerInfo.Credits = credits;
            gangs.GetGangsService().PushPlayerUpdate(playerInfo);
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_setcredits_success", executor.PlayerName, playerInfo.Credits);
            return;
        }

        TargetResult? target = GetTarget(info);
        if (target == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "Player not found.");
            return;
        }

        if(!int.TryParse(info.GetArg(2), out credits)) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "Invalid credits amount.");
            return;
        }

        foreach (var player in target.Players)
        {
            GangPlayer? playerInfo = gangs.GetGangsService().GetGangPlayer(player.SteamID).GetAwaiter()
                .GetResult();

            if (playerInfo == null)
            {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                    "Could not load information for player. Try again in a few seconds.");
                return;
            }

            playerInfo.Credits = credits;
            gangs.GetGangsService().PushPlayerUpdate(playerInfo);
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_setcredits_success", player.PlayerName, playerInfo.Credits);
        }
    }
}