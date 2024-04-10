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

public class GangMemberCmd(ICS2Gangs gangs) : Command(gangs)
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

        if(!ulong.TryParse(info.GetArg(0), out ulong targetSteamId))
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Invalid SteamID.");
            return;
        }

        GangPlayer? menuPlayer = gangs.GetGangsService().GetGangPlayer(targetSteamId).GetAwaiter().GetResult();

        if (menuPlayer == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Player not found in the database.");
            return;
        }

        if (menuPlayer.GangId == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Player is not in a gang.");
            return;
        }

        Gang? menuGang = gangs.GetGangsService().GetGang(menuPlayer.GangId.Value).GetAwaiter().GetResult();

        if (menuGang == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Player's gang was not found in the database.");
            return;
        }

        var menu = new GangMenuMember(
            gangs,
            gangs.GetGangsService(),
            gang,
            gangPlayer,
            menuGang,
            menuPlayer);

        MenuManager.OpenChatMenu(executor, (ChatMenu)menu.GetMenu());
    }
}