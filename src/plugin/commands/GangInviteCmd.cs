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

        var executorSteam = executor.AuthorizedSteamID;
        if (executorSteam == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Try again in a few seconds.");
            return;
        }

        int argCount = info.ArgCount;
        SteamID? targetSteam = null;
        CCSPlayerController? player = null;

        if(argCount > 1) {
            TargetResult? target = GetSingleTarget(info);
            if (target == null)
            {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "Player not found.");
                return;
            }

            player = target.First();
            targetSteam = player.AuthorizedSteamID;

            if (targetSteam == null)
            {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                    "Player has no authorized SteamID.");
                return;
            }
        }

        Task.Run(async () => {
            GangPlayer? senderPlayer = await gangs.GetGangsService().GetGangPlayer(executorSteam.SteamId64);
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

            if (argCount <= 1)
            {
                var menu = await new GangMenuInvite(gangs, gangs.GetGangsService(), senderGang, senderPlayer).GetMenu();
                Server.NextFrame(() => {
                    MenuManager.OpenChatMenu(executor, (ChatMenu)menu);
                });
                return;
            }

            GangPlayer? targetPlayer = await gangs.GetGangsService().GetGangPlayer(targetSteam.SteamId64);
            if (targetPlayer == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player not found in the database.");
                });
                return;
            }

            if (targetPlayer.GangId != null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is already in a gang.");
                });
                return;
            }

            if (senderPlayer.SteamId == targetPlayer.SteamId)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You cannot invite yourself!.");
                });
                return;
            }

            var gangMembers = await gangs.GetGangsService().GetGangMembers(senderGang.Id);
            var gangMembersCount = gangMembers.Count();

            if (gangMembersCount >= senderGang.MaxSize)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Your gang is full.");
                });
                return;
            }

            gangs.GetGangInviteService().SendInvite(executor, player, senderPlayer, targetPlayer, senderGang);
        });
    }
}

