using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
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
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Try again in a few seconds.");
            return;
        }

        if (info.ArgCount <= 1)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_usage",
                "css_gangmember <SteamID>");
            return;
        }

        if(!ulong.TryParse(info.GetArg(1), out ulong targetSteamId))
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Invalid SteamID.");
            return;
        }

        Task.Run(async () => {
            GangPlayer? gangPlayer = await gangs.GetGangsService().GetGangPlayer(steam.SteamId64);
            if (gangPlayer == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You were not found in the database. Try again in a few seconds.");
                });
                return;
            }
            if (gangPlayer.GangId == null) {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You are not in a gang.");
                });
                return;
            }

            Gang? gang = await gangs.GetGangsService().GetGang(gangPlayer.GangId.Value);
            if (gang == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Your gang was not found in the database. Try again in a few seconds.");
                });
                return;
            }

            GangPlayer? menuPlayer = await gangs.GetGangsService().GetGangPlayer(targetSteamId);
            if (menuPlayer == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player not found in the database.");
                });
                return;
            }
            if (menuPlayer.GangId == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is not in a gang.");
                });
                return;
            }

            Gang? menuGang = await gangs.GetGangsService().GetGang(menuPlayer.GangId.Value);
            if (menuGang == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player's gang was not found in the database.");
                });
                return;
            }

            var menu = await new GangMenuMember(
                gangs,
                gangs.GetGangsService(),
                gang,
                gangPlayer,
                menuGang,
                menuPlayer).GetMenu();

            Server.NextFrame(() => {
                MenuManager.OpenChatMenu(executor, (ChatMenu)menu);
            });
        });
    }
}