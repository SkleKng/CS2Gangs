using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using plugin.extensions;
using plugin.utils;

namespace plugin.commands;

public class GangPurchaseCmd(ICS2Gangs gangs) : Command(gangs)
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
                "css_gangpurchase <Type>");
            return;
        }

        if (!int.TryParse(info.GetArg(1), out int type))
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Invalid type.");
            return;
        }

        if (!Enum.IsDefined(typeof(GangPurchaseType), type))
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Invalid perk.");
            return;
        }

        Task.Run(async () =>
        {
            GangPlayer? gangPlayer = await gangs.GetGangsService().GetGangPlayer(steam.SteamId64);
            if (gangPlayer == null)
            {
                Server.NextFrame(() =>
                {
                    if (!executor.IsReal())
                        return;
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You were not found in the database. Try again in a few seconds.");
                });
                return;
            }

            if (gangPlayer.GangId == null)
            {
                Server.NextFrame(() =>
                {
                    if (!executor.IsReal())
                        return;
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You are not in a gang.");
                });
                return;
            }

            if (gangPlayer.GangRank < (int?)GangRank.Officer)
            {
                Server.NextFrame(() =>
                {
                    if (!executor.IsReal())
                        return;
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You must at least be an officer to purchase perks!");
                });
                return;
            }

            Gang? gang = await gangs.GetGangsService().GetGang(gangPlayer.GangId.Value);
            if (gang == null)
            {
                Server.NextFrame(() =>
                {
                    if (!executor.IsReal())
                        return;
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Your gang was not found in the database. Try again in a few seconds.");
                });
                return;
            }

            switch ((GangPurchaseType)type)
            {
                case GangPurchaseType.GangChat:
                    if (gang.Credits < gangs.Config.GangChatCost)
                    {
                        Server.NextFrame(() =>
                        {
                            if (!executor.IsReal())
                                return;
                            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                                "Not enough credits.");
                        });
                    }
                    else if (gang.Chat)
                    {
                        Server.NextFrame(() =>
                        {
                            if (!executor.IsReal())
                                return;
                            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                                "Gang chat is already unlocked.");
                        });
                    }
                    else
                    {
                        gang.Credits -= gangs.Config.GangChatCost;
                        gang.Chat = true;
                        gangs.GetGangsService().PushGangUpdate(gang);
                        Server.NextFrame(() =>
                        {
                            if (!executor.IsReal())
                                return;
                            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangpurchase_success",
                                "Gang Chat", gangs.Config.GangChatCost.ToString());
                            gangs.GetAnnouncerService().AnnounceToGangLocalized(gang, gangs.GetBase().Localizer,
                                "gang_announce_purchase", gangPlayer.PlayerName ?? "Unknown", "Gang Chat", gangs.Config.GangChatCost.ToString());
                        });
                    }
                    break;

                default:
                    Server.NextFrame(() =>
                    {
                        if (!executor.IsReal())
                            return;
                        executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                            "Invalid perk.");
                    });
                    break;
            }
        });
    }
}

public enum GangPurchaseType
{
    GangChat = 1
}