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

        GangPlayer? senderPlayer = gangs.GetGangsService().GetGangPlayer(steam.SteamId64).GetAwaiter()
            .GetResult();

        if (senderPlayer == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You were not found in the database. Try again in a few seconds.");
            return;
        }
        if (senderPlayer.GangId == null) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You are not in a gang.");
            return;
        }

        Gang? senderGang = gangs.GetGangsService().GetGang(senderPlayer.GangId.Value).GetAwaiter().GetResult();

        if (senderGang == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Your gang was not found in the database. Try again in a few seconds.");
            return;
        }

        if (senderPlayer.GangRank == (int?)GangRank.Member)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You must be at least an officer of the gang to invite a member.");
            return;
        }

        if (info.ArgCount <= 1)
        {
            var menu = new GangMenuInvite(gangs, gangs.GetGangsService(), senderGang, senderPlayer);
            MenuManager.OpenChatMenu(executor, (ChatMenu)menu.GetMenu());
            return;
        }

        TargetResult? targetResult = GetTarget(info);
        if(targetResult == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player not found.");
            return;
        }

        if(targetResult.Count() > 1)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Multiple players found. Be more specific.");
            return;
        }
        var target = targetResult.First();
        if(target.AuthorizedSteamID == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player has no authorized SteamID.");
            return;
        }

        GangPlayer? targetPlayer = gangs.GetGangsService().GetGangPlayer(target.AuthorizedSteamID.SteamId64).GetAwaiter().GetResult();

        if (targetPlayer == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player not found in the database.");
            return;
        }

        if (targetPlayer.GangId != null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player is already in a gang.");
            return;
        }

        if (senderPlayer.SteamId == targetPlayer.SteamId)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You cannot invite yourself!.");
            return;
        }

        gangs.GetGangInvites()[senderPlayer.SteamId] = targetPlayer.SteamId;
        AddExpireTimer(executor, target, senderPlayer, targetPlayer, senderGang);

        executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_ganginvite_success", targetPlayer.PlayerName ?? "Unknown");
        target.PrintLocalizedChat(gangs.GetBase().Localizer, "command_ganginvite_target", senderPlayer.PlayerName ?? "Unknown", senderGang.Name ?? "Unknown");
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

