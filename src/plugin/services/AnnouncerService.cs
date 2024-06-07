using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;
using plugin.utils;

namespace plugin.services;

public class AnnouncerService : IAnnouncerService
{
    private readonly CS2Gangs CS2Gangs;

    public AnnouncerService(CS2Gangs CS2Gangs)
    {
        this.CS2Gangs = CS2Gangs;
    }

    public void AnnounceToGang(Gang gang, string message)
    {
        _ = Task.Run(async () =>
        {
            IEnumerable<GangPlayer> members = await CS2Gangs.GetGangsService().GetGangMembers(gang.Id);
            foreach (GangPlayer member in members)
            {
                Server.NextFrame(() =>
                {
                    CCSPlayerController player = Utilities.GetPlayerFromSteamId((ulong)member.SteamId);
                    if (player != null)
                    {
                        player.PrintToChat(message);
                    }
                });
            }
        });        
    }

    public void AnnounceToGangLocalized(Gang gang, IStringLocalizer localizer, string local, params object[] args)
    {
        string message = localizer[local, args];
        message = message.Replace("%prefix%", localizer["prefix"]);
        message = StringUtils.ReplaceChatColors(message);
        AnnounceToGang(gang, message);
    }

    public void AnnounceToServer(string message)
    {
        Server.PrintToChatAll(message);
    }

    public void AnnounceToServerLocalized(IStringLocalizer localizer, string local, params object[] args)
    {
        string message = localizer[local, args];
        message = message.Replace("%prefix%", localizer["prefix"]);
        message = StringUtils.ReplaceChatColors(message);
        AnnounceToServer(message);
    }
}