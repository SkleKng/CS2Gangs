using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using plugin.extensions;

namespace plugin.commands;

public class GangDepositCmd(ICS2Gangs gangs) : Command(gangs)
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
        if(info.ArgCount <= 1) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_usage", "css_gangdeposit <amount>");
            return;
        }

        int credits = 0;
        if (!int.TryParse(info.GetArg(1), out credits)) {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "Invalid credits amount.");
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
            if (senderPlayer.Credits < credits) {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You do not have enough credits.");
                });
                return;
            }
            Gang? gang = await gangs.GetGangsService().GetGang(senderPlayer.GangId.Value);
            if (gang == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Your gang was not found in the database. Try again in a few seconds.");
                });
                return;
            }
            
            Server.NextFrame(() => {
                gangs.GetCreditService().DepositInBank(executor, senderPlayer, gang, credits);
            });
        });
    }
}