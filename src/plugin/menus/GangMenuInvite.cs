using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.services;
using plugin.utils;

namespace plugin.menus;

public class GangMenuInvite(ICS2Gangs gangs, IGangsService gangService, Gang? gang, GangPlayer gangPlayer) : GangMenu(gangs, gangService, gang, gangPlayer)
{
    public override IMenu GetMenu()
    {
        IMenu menu; 
        if(gang == null) {
            menu = new ChatMenu("This should not happen.");
            menu.AddMenuOption($"Please let TechLE know if this shows up", emptyAction(), true);
            return menu;
        }

        menu = new ChatMenu($"{gang.Name} - Invite a Player");
        
        foreach (var player in Utilities.GetPlayers())
        {
            if(!player.IsReal())
                continue;
            if(player.AuthorizedSteamID == null)
                continue;
            if(player.AuthorizedSteamID.SteamId64 == (ulong)gangPlayer.SteamId)
                continue;
            GangPlayer? newPlayer = gangs.GetGangsService().GetGangPlayer(player.AuthorizedSteamID.SteamId64).GetAwaiter().GetResult();
            if(newPlayer == null)
                continue;
            if(newPlayer.GangId != null)
                continue;
            menu.AddMenuOption($"{newPlayer.PlayerName ?? "Unknown"}", generateCommandAction($"css_ganginvite {newPlayer.SteamId}"));
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