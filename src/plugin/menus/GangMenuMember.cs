using api.plugin;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.services;
using plugin.utils;

namespace plugin.menus;

public class GangMenuMember(ICS2Gangs gangs, IGangsService gangService, Gang? senderGang, GangPlayer sender, Gang menuGang, GangPlayer menuPlayer) : GangMenu(gangs, gangService, senderGang, sender)
{
    public override IMenu GetMenu()
    {
        IMenu menu; 

        menu = new ChatMenu($"{menuGang.Name} - {menuPlayer.PlayerName}");
        
        menu.AddMenuOption($"Invited by: {menuPlayer.InvitedBy}", emptyAction(), true);
        menu.AddMenuOption($"Rank: {GangUtils.GetGangRankName(menuPlayer.GangRank)}", emptyAction(), true);
        menu.AddMenuOption("", emptyAction(), true);
        menu.AddMenuOption("Kick", generateCommandAction($"css_gangkick {menuPlayer.SteamId}"), sender.GangRank <= menuPlayer.GangRank);
        menu.AddMenuOption("Promote", generateCommandAction($"css_gangpromote {menuPlayer.SteamId}"), sender.GangRank != (int?)GangRank.Owner);
        menu.AddMenuOption("Demote", generateCommandAction($"css_gangdemote {menuPlayer.SteamId}"), sender.GangRank != (int?)GangRank.Owner);
        menu.AddMenuOption("Transfer Ownership", generateCommandAction($"css_gangtransfer {menuPlayer.SteamId}"), sender.GangRank == (int?)GangRank.Owner);

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