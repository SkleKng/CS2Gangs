using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Modules.Menu;

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
        var menu = new CenterHtmlMenu($"{gang?.Name ?? "Unassigned"} - Gang Perks");
        return menu;
    }
}