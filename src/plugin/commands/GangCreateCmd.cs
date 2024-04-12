using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;
using plugin.services;
using plugin.utils;

namespace plugin.commands;

public class GangCreateCmd(ICS2Gangs gangs) : Command(gangs)
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

        if(info.ArgCount <= 1) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangcreation_explain");
            return;
        }

        string name = info.GetArg(1);

        if (gangs.GetGangsService().GangNameExists(name).GetAwaiter().GetResult())
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangcreation_nameexists");
            return;
        }

        var steam = executor.AuthorizedSteamID;
        if (steam == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Try again in a few seconds.");
            return;
        }

        GangPlayer? gangPlayer = gangs.GetGangsService().GetGangPlayer(steam.SteamId64).GetAwaiter()
            .GetResult();

        if (gangPlayer == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You were not found in the database. Try again in a few seconds.");
            return;
        }
        if (gangPlayer.GangId != null) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You are already in a gang. Leave your current gang to create a new one.");
            return;
        }

        if (gangPlayer.Credits < gangs.Config.GangCreationPrice)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You do not have enough credits to create a gang.");
            return;
        }

        Gang newGang = new Gang(gangs.GetGangsService().GetNextGangId().GetAwaiter().GetResult(), name);
        gangPlayer.GangId = newGang.Id;
        gangPlayer.InvitedBy = gangPlayer.PlayerName;
        gangPlayer.GangRank = (int?)GangRank.Owner;
        gangPlayer.Credits -= gangs.Config.GangCreationPrice;

        gangs.GetGangsService().PushGangUpdate(newGang);
        gangs.GetGangsService().PushPlayerUpdate(gangPlayer);
        
        executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangcreation_success", name);
    }
}