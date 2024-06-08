using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace plugin.menus;

public class GangMenuBank(ICS2Gangs gangs, IGangsService gangService, Gang gang, GangPlayer player) : GangMenu(gangs, gangService, gang, player)
{
    public override async Task<IMenu> GetMenu()
    {
        IMenu menu;
        if (gang == null)
        {
            menu = new ChatMenu("This should not happen.");
            menu.AddMenuOption($"Please let TechLE know if this shows up", emptyAction(), true);
            return menu;
        }

        menu = new ChatMenu($"{gang.Name} - Bank");
        menu.AddMenuOption($"Balance: {gang.Credits}", emptyAction(), true);

        menu.AddMenuOption($"Deposit 100", generateCommandAction($"css_gangdeposit 100"), player.Credits < 100);
        menu.AddMenuOption($"Deposit 1000", generateCommandAction($"css_gangdeposit 1000"), player.Credits < 1000);
        menu.AddMenuOption($"Deposit 10000", generateCommandAction($"css_gangdeposit 10000"), player.Credits < 10000);
        return menu;
    }

    private Action<CCSPlayerController, ChatMenuOption> emptyAction()
    {
        return (player, _) => { };
    }

    private Action<CCSPlayerController, ChatMenuOption> generateCommandAction(string cmd)
    {
        return (player, _) => { player.ExecuteClientCommandFromServer(cmd); };
    }
}