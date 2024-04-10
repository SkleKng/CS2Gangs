using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.services;
using plugin.utils;

namespace plugin.menus;

public class GangMenuMembers(ICS2Gangs gangs, IGangsService gangService, Gang? gang, GangPlayer player) : GangMenu(gangs, gangService, gang, player)
{
    public override IMenu GetMenu()
    {
        IMenu menu; 
        if(gang == null) {
            menu = new ChatMenu("This should not happen.");
            menu.AddMenuOption($"Please let TechLE know if this shows up", (CCSPlayerController controller, ChatMenuOption option) => {}, true);
            return menu;
        }

        menu = new ChatMenu($"{gang.Name} - Members");
        
        foreach (var member in gangs.GetGangsService().GetGangMembers(gang.Id).GetAwaiter().GetResult())
        {
            menu.AddMenuOption($"{member.PlayerName ?? "Unknown"} - {GangUtils.GetGangRankName(member.GangRank)}", generateCommandAction($"css_gangmember {member.SteamId}"));
        }

        return menu;
    }

    private Action<CCSPlayerController, ChatMenuOption> generateCommandAction(string cmd)
    {
        return (player, _) => { player.ExecuteClientCommandFromServer(cmd); };
    }
}