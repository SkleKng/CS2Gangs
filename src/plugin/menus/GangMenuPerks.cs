using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.commands;

namespace plugin.menus;

/// <summary>
/// This menu displays the perks that a gang has.
/// Since it is dependent on a gang, the gang cannot be null.
/// </summary>
/// <param name="gangService"></param>
/// <param name="gang"></param>
/// <param name="player"></param>
public class GangMenuPerks(ICS2Gangs gangs, IGangsService gangService, Gang gang, GangPlayer player) : GangMenu(gangs, gangService, gang, player)
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

        menu = new ChatMenu($"{gang.Name} - Perks");
        menu.AddMenuOption($"Balance: {gang.Credits}", emptyAction(), true);

        menu.AddMenuOption($"Gang Chat - {(gang.Chat ? "Owned" : gangs.Config.GangChatCost + " Credits")}", generateCommandAction($"css_gangpurchase 1"), gang.Credits < gangs.Config.GangChatCost || gang.Chat);
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