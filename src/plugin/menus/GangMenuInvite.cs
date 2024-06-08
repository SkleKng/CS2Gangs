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
    public override async Task<IMenu> GetMenu()
    {
        IMenu menu; 
        if(gang == null) {
            menu = new ChatMenu("This should not happen.");
            menu.AddMenuOption($"Please let TechLE know if this shows up", emptyAction(), true);
            return menu;
        }

        menu = new ChatMenu($"{gang.Name} - Invite a Player");

        List<Task<GangPlayer?>> tasks = new List<Task<GangPlayer?>>();

        await Server.NextFrameAsync(() =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!player.IsReal())
                    continue;
                if (player.AuthorizedSteamID == null)
                    continue;
                if (player.AuthorizedSteamID.SteamId64 == (ulong)gangPlayer.SteamId)
                    continue;
                tasks.Add(gangService.GetGangPlayer(player.AuthorizedSteamID.SteamId64));

                // GangPlayer? newPlayer = await gangs.GetGangsService().GetGangPlayer(player.AuthorizedSteamID.SteamId64);
                // if(newPlayer == null)
                //     continue;
                // if(newPlayer.GangId != null)
                //     continue;

            }
        });

        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            GangPlayer? newPlayer = task.Result;
            if(newPlayer == null)
                continue;
            if(newPlayer.GangId != null)
                continue;

            menu.AddMenuOption($"{newPlayer.PlayerName ?? "Unknown"}", generateCommandAction($"css_ganginvite {newPlayer.PlayerName}"));
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