using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;
using plugin.services;
using plugin.utils;

namespace plugin.commands;

public class GangInviteCommand(ICS2Gangs gangs) : Command(gangs)
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

            if (senderPlayer.GangRank == (int?)GangRank.Member)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You must be at least an officer of the gang to invite a member.");
                });
                return;
            }

            if (info.ArgCount <= 1)
            {
                var menu = new GangMenuInvite(gangs, gangs.GetGangsService(), senderGang, senderPlayer);
                Server.NextFrame(() => {
                    MenuManager.OpenChatMenu(executor, (ChatMenu)menu.GetMenu());
                });
                return;
            }

            TargetResult? target = GetTarget(info); // this should really be a single target
            if (target == null)
            {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "Player not found.");
                return;
            }

            foreach (var player in target.Players)
            {
                if (player.AuthorizedSteamID == null)
                {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player has no authorized SteamID.");
                    continue;
                }

                GangPlayer? targetPlayer = await gangs.GetGangsService().GetGangPlayer(player.AuthorizedSteamID.SteamId64);
                if (targetPlayer == null)
                {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player not found in the database.");
                    continue;
                }

                if (targetPlayer.GangId != null)
                {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is already in a gang.");
                    continue;
                }

                if (senderPlayer.SteamId == targetPlayer.SteamId)
                {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You cannot invite yourself!.");
                    continue;
                }

                gangs.GetGangInvites()[senderPlayer.SteamId] = targetPlayer.SteamId;
                AddExpireTimer(executor, player, senderPlayer, targetPlayer, senderGang);

                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_ganginvite_success", targetPlayer.PlayerName ?? "Unknown");
                player.PrintLocalizedChat(gangs.GetBase().Localizer, "command_ganginvite_target", senderPlayer.PlayerName ?? "Unknown", senderGang.Name ?? "Unknown");
            }
        });
    }

    private void AddExpireTimer(CCSPlayerController sender, CCSPlayerController target, GangPlayer senderPlayer, GangPlayer targetPlayer, Gang gang)
    {
        int inviteTime = gangs.Config.GangInviteExpireMinutes;
        Server.NextFrame(() =>
        {
            gangs.GetBase().AddTimer(inviteTime * 60, () =>
            {
                if (senderPlayer.GangId == null || targetPlayer.GangId == null)
                    return;
                if (senderPlayer.GangId != targetPlayer.GangId)
                    return;
                gangs.GetGangInvites().Remove(senderPlayer.SteamId);
                sender.PrintLocalizedChat(gangs.GetBase().Localizer, "command_ganginvite_expire_sender", targetPlayer.PlayerName ?? "Unknown");
                target.PrintLocalizedChat(gangs.GetBase().Localizer, "command_ganginvite_expire_receiver", gang.Name);
            });
        });
    }
}

