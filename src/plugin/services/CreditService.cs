using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using plugin.extensions;

namespace plugin.services;

public class CreditService : ICreditService
{
    private readonly CS2Gangs CS2Gangs;

    public CreditService(CS2Gangs CS2Gangs)
    {
        this.CS2Gangs = CS2Gangs;
        CS2Gangs.GetBase().AddTimer(CS2Gangs.Config!.CreditsDeliveryInterval, CreditTimer, TimerFlags.REPEAT);
    }

    private void CreditTimer()
    {
        List<Task<GangPlayer?>> tasks = new();

        List<int> creditsToGive = new();

        var players = Utilities.GetPlayers();
        if(players.Count == 0)
            return;

        foreach (var player in players)
        {
            if (!player.IsReal())
                continue;
            if (player.AuthorizedSteamID == null)
                continue;
            tasks.Add(CS2Gangs.GetGangsService().GetGangPlayer(player.AuthorizedSteamID.SteamId64));
            switch (player.GetVIPTier(CS2Gangs.Config!))
            {
                case 1:
                    creditsToGive.Add(CS2Gangs.Config!.VIPTier1PassiveCreditAmount);
                    break;
                case 2:
                    creditsToGive.Add(CS2Gangs.Config!.VIPTier2PassiveCreditAmount);
                    break;
                case 3:
                    creditsToGive.Add(CS2Gangs.Config!.VIPTier3PassiveCreditAmount);
                    break;
                case 4:
                    creditsToGive.Add(CS2Gangs.Config!.VIPTier4PassiveCreditAmount);
                    break;
                default:
                    creditsToGive.Add(CS2Gangs.Config!.DefaultPassiveCreditAmount);
                    break;
            }
        }

        Task.Run(async () => {
            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                GangPlayer? gangPlayer = task.Result;
                if (gangPlayer == null)
                    continue;

                gangPlayer.Credits += creditsToGive[tasks.IndexOf(task)];
                CS2Gangs.GetGangsService().PushPlayerUpdate(gangPlayer);
                Server.NextFrame(() => {
                    CCSPlayerController player = Utilities.GetPlayerFromSteamId((ulong)gangPlayer.SteamId);
                    if (player != null)
                    {
                        player.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "credits_earned", creditsToGive[tasks.IndexOf(task)], gangPlayer.Credits);
                    }
                });
            }
        });
    }

    public void DepositInBank(CCSPlayerController player, GangPlayer gangPlayer, Gang gang, int amount)
    {
        gangPlayer.Credits -= amount;
        gang.Credits += amount;
        CS2Gangs.GetGangsService().PushPlayerUpdate(gangPlayer);
        CS2Gangs.GetGangsService().PushGangUpdate(gang);

        Server.NextFrame(() => {
            player.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "credits_deposited", amount, gangPlayer.Credits);
            CS2Gangs.GetAnnouncerService().AnnounceToGangLocalized(gang, CS2Gangs.GetBase().Localizer, "gang_announce_deposit", gangPlayer.PlayerName ?? "Unknown", amount);
        });
    }
}