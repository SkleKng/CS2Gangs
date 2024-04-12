using api.plugin;
using api.plugin.models;
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

public class GangJoinCommand(ICS2Gangs gangs) : Command(gangs)
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
        
        if (senderPlayer.GangId != null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You are already in a gang.");
            return;
        }

        if (info.ArgCount <= 1)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_usage",
                "css_gangjoin <player>");
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

        if(gangs.GetGangInvites().ContainsKey(targetPlayer.SteamId) == false) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You have not been invited to a gang by this player!");
            return;
        }

        if(gangs.GetGangInvites()[targetPlayer.SteamId] != senderPlayer.SteamId) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You have not been invited to a gang by this player!");
            return;
        }

        if(targetPlayer.GangId == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player is not in a gang.");
            return;
        }

        Gang? targetGang = gangs.GetGangsService().GetGang(targetPlayer.GangId.Value).GetAwaiter().GetResult();
        if (targetGang == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player's gang was not found in the database. This really shouldn't happen, contact TechLE if it does.");
            return;
        }

        gangs.GetGangInvites().Remove(targetPlayer.SteamId);
        senderPlayer.GangId = targetPlayer.GangId;
        senderPlayer.GangRank = (int?)GangRank.Member;
        senderPlayer.InvitedBy = targetPlayer.PlayerName;

        executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangjoin_success", targetGang.Name);

        gangs.GetGangsService().PushPlayerUpdate(senderPlayer);
    }
}