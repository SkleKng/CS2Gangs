using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.services;

namespace plugin.menus;

public class GangMenuBase(ICS2Gangs gangs, IGangsService gangService, Gang? gang, GangPlayer player) : GangMenu(gangs, gangService, gang, player)
{
    public override IMenu GetMenu()
    {
        IMenu menu; 
        if(gang == null) {
            menu = new ChatMenu("Gang Creation");
            menu.AddMenuOption($"Create Gang - {player.Credits}/{gangs.Config.GangCreationPrice} Credits", generateCommandAction("css_gangcreate"), player.Credits < gangs.Config.GangCreationPrice);
            return menu;
        }

        menu = new ChatMenu($"{gang.Name} - Gang Menu");
        menu.AddMenuOption("View Gang Perks", generateCommandAction($"css_gang_perks {gang.Id}"));
        menu.AddMenuOption("View Gang Members", generateCommandAction($"css_gang_members {gang.Id}"));
        menu.AddMenuOption("Leave Gang", generateCommandAction($"css_gang_leave {gang.Id}"));
        return menu;
        
    }

    private Action<CCSPlayerController, ChatMenuOption> generateCommandAction(string cmd)
    {
        return (player, _) => { player.ExecuteClientCommandFromServer(cmd); };
    }
}