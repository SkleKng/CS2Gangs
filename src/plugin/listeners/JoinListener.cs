using api.plugin;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using plugin.extensions;

namespace plugin.listeners;

public class JoinListener
{
    private readonly ICS2Gangs gangs;

    public JoinListener(ICS2Gangs gangs)
    {
        this.gangs = gangs;

        gangs.GetBase().RegisterEventHandler<EventPlayerConnectFull>(OnPlayerJoin);
    }

    public HookResult OnPlayerJoin(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsReal())
            return HookResult.Continue;

        if (player.AuthorizedSteamID == null)
        {
            gangs.GetBase().Logger.LogError("Gangs - Player {player} has no authorized SteamID", player.PlayerName);
            player.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Please rejoin if you intend to use gangs functionality. If the problem persists, contact TechLE.");
            return HookResult.Continue;
        }

        gangs.GetGangsService().UpdatePlayerOnJoin(player.AuthorizedSteamID.SteamId64, player.PlayerName);

        return HookResult.Continue;
    }
}