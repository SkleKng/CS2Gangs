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

public class GangTransferCmd(ICS2Gangs gangs) : Command(gangs)
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

        if(!ulong.TryParse(info.GetArg(1), out ulong targetSteamId))
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Invalid SteamID.");
            return;
        }

        GangPlayer? targetPlayer = gangs.GetGangsService().GetGangPlayer(targetSteamId).GetAwaiter().GetResult();

        if (targetPlayer == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player not found in the database.");
            return;
        }

        if (targetPlayer.GangId == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player is not in a gang.");
            return;
        }

        Gang? targetGang = gangs.GetGangsService().GetGang(targetPlayer.GangId.Value).GetAwaiter().GetResult();

        if (targetGang == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player's gang was not found in the database.");
            return;
        }

        if (senderPlayer.GangRank != (int?)GangRank.Owner)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You must be the owner to transfer ownership!");
            return;
        }

        if (senderPlayer.GangId != targetPlayer.GangId)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player is not in your gang.");
            return;
        }

        if (senderPlayer.SteamId == targetPlayer.SteamId)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "You cannot transfer ownership to yourself!.");
            return;
        }

        if (targetPlayer.GangRank != (int?)GangRank.Officer)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Player is not a gang officer. Consider if you really want to transfer ownership to this player if they are not yet an officer.");
            return;
        }

        targetPlayer.GangRank = (int)GangRank.Owner;
        senderPlayer.GangRank = (int)GangRank.Officer;

        gangs.GetGangsService().PushPlayerUpdate(targetPlayer);
        gangs.GetGangsService().PushPlayerUpdate(senderPlayer);

        executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangtransfer_success", targetPlayer.PlayerName ?? "Unknown");
    }
}