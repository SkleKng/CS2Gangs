using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.services;

namespace plugin.menus;

public abstract class GangMenu(ICS2Gangs gangs, IGangsService gangService, Gang? gang, GangPlayer player)
{
    protected readonly ICS2Gangs gangs = gangs;

    /// <summary>
    ///  The service that provides access to the gangs database.
    /// </summary>
    protected readonly IGangsService gangService = gangService;

    /// <summary>
    /// The gang that this menu is for.
    /// Note that the player may not be in this gang.
    /// </summary>
    protected readonly Gang? gang = gang;

    /// <summary>
    /// The player that is viewing the menu.
    /// </summary>
    protected readonly GangPlayer player = player;

    public abstract IMenu GetMenu();
}