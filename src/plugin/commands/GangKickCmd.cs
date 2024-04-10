using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;
using plugin.services;
using plugin.utils;

namespace plugin.commands;

public class GangKickCmd(ICS2Gangs gangs) : Command(gangs)
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

        GangPlayer? senderPlayer = gangs.GetGangsService().GetGangPlayer(steam.SteamId64).GetAwaiter()
            .GetResult();

        if (senderPlayer == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You were not found in the database. Try again in a few seconds.");
            return;
        }
        if (senderPlayer.GangId == null) {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You are not in a gang.");
            return;
        }

        Gang? senderGang = gangs.GetGangsService().GetGang(senderPlayer.GangId.Value).GetAwaiter().GetResult();

        if (senderGang == null)
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

        GangPlayer? targetPlayer = gangs.GetGangsService().GetGangPlayer(targetSteamId).GetAwaiter().GetResult();

        if (targetPlayer == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Player not found in the database.");
            return;
        }

        if (targetPlayer.GangId == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Player is not in a gang.");
            return;
        }

        Gang? targetGang = gangs.GetGangsService().GetGang(targetPlayer.GangId.Value).GetAwaiter().GetResult();

        if (targetGang == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Player's gang was not found in the database.");
            return;
        }

        if (senderPlayer.GangRank <= (int?)GangRank.Member)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You must at least be an officer to kick a player.");
            return;
        }

        if (senderPlayer.GangId != targetPlayer.GangId)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "Player is not in your gang.");
            return;
        }

        if (senderPlayer.SteamId == targetPlayer.SteamId)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You cannot transfer ownership to yourself!.");
            return;
        }

        if (targetPlayer.GangRank >= senderPlayer.GangRank)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "You cannot kick a player with the same or higher rank than you.");
            return;
        }

        targetPlayer.GangId = null;
        targetPlayer.GangRank = null;
        targetPlayer.InvitedBy = null;

        gangs.GetGangsService().PushPlayerUpdate(targetPlayer);

        info.ReplyLocalized(gangs.GetBase().Localizer, "command_gangkick_success", targetPlayer.PlayerName ?? "Unknown");
    }
}