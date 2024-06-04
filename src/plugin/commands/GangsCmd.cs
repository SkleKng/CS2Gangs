using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;

namespace plugin.commands;

public class GangsCmd(ICS2Gangs gangs) : Command(gangs)
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

        Task.Run(async () => {
            GangPlayer? playerInfo = await gangs.GetGangsService().GetGangPlayer(steam.SteamId64);
            if (playerInfo == null)
            {
                Server.NextFrame(() => { // Run this on the game thread
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You were not found in the database. Try again in a few seconds.");
                });
                return;
            }

            var menu = new GangMenuBase(
                gangs,
                gangs.GetGangsService(),
                playerInfo.GangId == null ? null : gangs.GetGangsService().GetGang(playerInfo.GangId.Value).GetAwaiter().GetResult(),
                playerInfo);

            Server.NextFrame(() => {
                MenuManager.OpenChatMenu(executor, (ChatMenu)menu.GetMenu());
            });
        });
    }
}