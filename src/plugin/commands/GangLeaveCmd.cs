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
using plugin.utils;

namespace plugin.commands;

public class GangLeaveCmd(ICS2Gangs gangs) : Command(gangs)
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

            if (gangPlayer.GangRank == (int?)GangRank.Owner)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You are the owner of the gang. You must transfer ownership before leaving.");
                });
                return;
            }

            gangPlayer.GangId = null;
            gangPlayer.GangRank = (int)GangRank.Member;
            gangPlayer.InvitedBy = null;

            gangs.GetGangsService().PushPlayerUpdate(gangPlayer);

            Server.NextFrame(() => {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangleave_success", gang.Name);
                gangs.GetAnnouncerService().AnnounceToGangLocalized(gang, gangs.GetBase().Localizer, "gang_announce_leave", gangPlayer.PlayerName ?? "Unknown");
            });
        });
    }
}