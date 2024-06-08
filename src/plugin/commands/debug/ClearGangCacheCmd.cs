using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;

namespace plugin.commands;

public class ClearGangCacheCmd(ICS2Gangs gangs) : Command(gangs)
{
    public override void OnCommand(CCSPlayerController? executor, CommandInfo info)
    {
        if(executor != null) {
            if (!executor.IsReal())
            return;
            var steam = executor.AuthorizedSteamID;
            if (steam == null)
            {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                    "SteamID not authorized yet. Try again in a few seconds.");
                return;
            }
            if (!AdminManager.PlayerHasPermissions(executor, gangs.Config.DebugPermission!))
            {
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error", "You do not have permission to use this command.");
                return;
            }
        }
        gangs.GetGangsService().ClearCache();

        if(executor == null) {
            Server.PrintToConsole("Gangs cache cleared.");
            return;
        }
        executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_clearcache_success");
    }
}