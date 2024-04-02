using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;
using plugin.services;

namespace plugin.commands;

public class GangDisbandCmd(ICS2Gangs gangs) : Command(gangs)
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

        GangPlayer? gangPlayer = gangs.GetGangsService().GetGangPlayer(steam.SteamId64).GetAwaiter()
            .GetResult();

        if (gangPlayer == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You were not found in the database. Try again in a few seconds.");
            return;
        }
        if (gangPlayer.GangId == null) {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You are not in a gang.");
            return;
        }

        Gang? gang = gangs.GetGangsService().GetGang(gangPlayer.GangId.Value).GetAwaiter().GetResult();

        if (gang == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Your gang was not found in the database. Try again in a few seconds.");
            return;
        }

        if (gangPlayer.GangRank != 2) // owner
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You are not the owner of the gang!");
            return;
        }

        gangs.GetGangsService().DisbandGang(gang);
        info.ReplyLocalized(gangs.GetBase().Localizer, "command_gangdisband_success", gang.Name);
    }
}