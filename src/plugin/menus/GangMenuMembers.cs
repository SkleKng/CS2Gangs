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
    public override async Task<IMenu> GetMenu()
    {
        IMenu menu; 
        if(gang == null) {
            menu = new ChatMenu("This should not happen.");
            menu.AddMenuOption($"Please let TechLE know if this shows up", emptyAction(), true);
            return menu;
        }

        menu = new ChatMenu($"{gang.Name} - Members");

        IEnumerable<GangPlayer> members = await gangs.GetGangsService().GetGangMembers(gang.Id);
        
        //sort members by rank
        members = members.OrderByDescending(m => m.GangRank);

        foreach (var member in members)
        {
            menu.AddMenuOption($"{member.PlayerName ?? "Unknown"} - {GangUtils.GetGangRankName(member.GangRank)}", generateCommandAction($"css_gangmember {member.SteamId}"));
        }

        return menu;
    }

    private Action<CCSPlayerController, ChatMenuOption> emptyAction()
    {
        return (player, _) => {};
    }

    private Action<CCSPlayerController, ChatMenuOption> generateCommandAction(string cmd)
    {
        return (player, _) => { player.ExecuteClientCommandFromServer(cmd); };
    }
}