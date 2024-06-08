using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.services;
using plugin.utils;

namespace plugin.menus;

public class GangMenuBase(ICS2Gangs gangs, IGangsService gangService, Gang? gang, GangPlayer player) : GangMenu(gangs, gangService, gang, player)
{
    public override async Task<IMenu> GetMenu()
    {
        IMenu menu; 
        if(gang == null) {
            menu = new ChatMenu("Gang Creation");
            menu.AddMenuOption($"Create Gang - {player.Credits}/{gangs.Config.GangCreationPrice} Credits", generateCommandAction("css_gangcreate"), player.Credits < gangs.Config.GangCreationPrice);
            return menu;
        }

        menu = new ChatMenu($"{gang.Name} - Gang Menu");
        // menu.AddMenuOption("View Gang Perks", generateCommandAction($"css_gangperks"));
        menu.AddMenuOption("View Gang Members", generateCommandAction($"css_gangmembers"));

        if(player.GangRank < (int?)GangRank.Owner) { 
            menu.AddMenuOption("Leave Gang", generateCommandAction($"css_gangleave"));
        }
        else {
            menu.AddMenuOption("Disband Gang", generateCommandAction($"css_gangdisband"));
        }

        var gangMembers = await gangs.GetGangsService().GetGangMembers(gang.Id);
        var gangMembersCount = gangMembers.Count();

        menu.AddMenuOption("Invite Players", generateCommandAction($"css_ganginvite"), player.GangRank == (int?)GangRank.Member || gangMembersCount >= gang.MaxSize);
        menu.AddMenuOption("Gang Bank", generateCommandAction($"css_gangbank"));
        menu.AddMenuOption("Gang Perks", generateCommandAction($"css_gangperks"));

        return menu;
    }

    private Action<CCSPlayerController, ChatMenuOption> generateCommandAction(string cmd)
    {
        return (player, _) => { player.ExecuteClientCommandFromServer(cmd); };
    }
}