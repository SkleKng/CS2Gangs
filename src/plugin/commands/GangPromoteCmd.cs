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

public class GangPromoteCmd(ICS2Gangs gangs) : Command(gangs)
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
                "css_gangpromote <SteamID>");
            return;
        }

        if(!ulong.TryParse(info.GetArg(1), out ulong targetSteamId))
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Invalid SteamID.");
            return;
        }

        Task.Run(async () => {
            GangPlayer? senderPlayer = await gangs.GetGangsService().GetGangPlayer(steam.SteamId64);
            if (senderPlayer == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You were not found in the database. Try again in a few seconds.");
                });
                return;
            }
            if (senderPlayer.GangId == null) {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You are not in a gang.");
                });
                return;
            }

            Gang? senderGang = await gangs.GetGangsService().GetGang(senderPlayer.GangId.Value);
            if (senderGang == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Your gang was not found in the database. Try again in a few seconds.");
                });
                return;
            }

            GangPlayer? targetPlayer = await gangs.GetGangsService().GetGangPlayer(targetSteamId);
            if (targetPlayer == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player not found in the database.");
                });
                return;
            }

            if (targetPlayer.GangId == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is not in a gang.");
                });
                return;
            }

            Gang? targetGang = await gangs.GetGangsService().GetGang(targetPlayer.GangId.Value);
            if (targetGang == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player's gang was not found in the database.");
                });
                return;
            }

            if (senderPlayer.GangRank != (int?)GangRank.Owner)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You must be the owner of the gang to promote a member.");
                });
                return;
            }

            if (senderPlayer.GangId != targetPlayer.GangId)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is not in your gang.");
                });
                return;
            }

            if (senderPlayer.SteamId == targetPlayer.SteamId)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You cannot promote yourself!.");
                });
                return;
            }

            if (targetPlayer.GangRank == (int?)GangRank.Officer)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is already an officer. Use the transfer ownership command to transfer ownership.");
                });
                return;
            }

            targetPlayer.GangRank = (int)GangRank.Officer;

            gangs.GetGangsService().PushPlayerUpdate(targetPlayer);

            Server.NextFrame(() => {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangpromote_success", targetPlayer.PlayerName ?? "Unknown");
            });

        });
    }
}