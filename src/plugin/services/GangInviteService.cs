using System.ComponentModel;
using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using plugin.extensions;

namespace plugin.services;

public class GangInviteService : IGangInviteService
{
    private readonly CS2Gangs CS2Gangs;
    private List<GangInvite> GangInvites;

    public GangInviteService(CS2Gangs CS2Gangs)
    {
        this.CS2Gangs = CS2Gangs;
        GangInvites = new List<GangInvite>();
    }

    public void SendInvite(CCSPlayerController sender, CCSPlayerController receiver, GangPlayer senderGangPlayer, GangPlayer receiverGangPlayer, Gang gang)
    {
        long inviterId = senderGangPlayer.SteamId;
        string inviterName = senderGangPlayer.PlayerName ?? "Unknown";
        long inviteeId = receiverGangPlayer.SteamId;
        string inviteeName = receiverGangPlayer.PlayerName ?? "Unknown";
        int gangId = gang.Id;
        string gangName = gang.Name;
        DateTime invTime = DateTime.Now;

        //check if there is already an invite from sender to receiver
        if (GangInvites.Any(i => i.inviterId == inviterId && i.inviteeId == inviteeId))
        {
            Server.NextFrame(() => {
                CS2Gangs.Logger.LogInformation($"Invite already sent from {inviterName} to {inviteeName} for gang {gangName}");
                sender.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_ganginvite_already_sent", inviteeName);
            });
            return;
        }

        GangInvite invite = new GangInvite(inviterId, inviterName, inviteeId, inviteeName, gangId, gangName, invTime);
        GangInvites.Add(invite);

        CS2Gangs.Logger.LogInformation($"Invite sent from {inviterName} to {inviteeName} for gang {gangName}");

        Server.NextFrame(() => {
            sender.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_ganginvite_sent", inviteeName, gangName);

            receiver.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_ganginvite_receiver", inviterName, gangName);

            Server.RunOnTick(Server.TickCount + CS2Gangs.Config!.GangInviteExpireMinutes * 60 * 64, () =>
            {
                if (GangInvites.Contains(invite))
                {
                    GangInvites.Remove(invite);
                    CS2Gangs.Logger.LogInformation($"Invite from {inviterName} to {inviteeName} for gang {gangName} expired");
                    sender.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_ganginvite_expire_sender", inviteeName);
                    receiver.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_ganginvite_expire_receiver", gangName);
                }
            });
        });
        return;
    }

    public void AcceptInvite(CCSPlayerController invitee, GangPlayer inviteeGangPlayer, string? gangName)
    {
        GangInvite? invite = null;
        if(gangName == null) {
            List<GangInvite> invites = GangInvites.FindAll(i => i.inviteeId == inviteeGangPlayer.SteamId);
            if(invites.Count == 0) {
                Server.NextFrame(() => {
                    invitee.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_gangjoin_no_invites");
                });
                return;
            }
            if(invites.Count > 1) {
                Server.NextFrame(() => {
                    invitee.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_gangjoin_multiple_invites");
                });
                return;
            }
            invite = invites[0];
        }
        else {
            invite = GangInvites.FirstOrDefault(i => i.inviteeId == inviteeGangPlayer.SteamId && i.gangName.Contains(gangName, StringComparison.OrdinalIgnoreCase));
        }

        if (invite == null)
        {
            Server.NextFrame(() => {
                CS2Gangs.Logger.LogInformation($"Invite not found for {inviteeGangPlayer.PlayerName} to join {gangName!}");
                invitee.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_gangjoin_not_found", gangName!);
            });
            return;
        }

        gangName = invite.gangName;

        GangInvites.Remove(invite);

        inviteeGangPlayer.GangId = invite.gangId;
        inviteeGangPlayer.GangRank = 0;
        inviteeGangPlayer.InvitedBy = invite.inviterName;

        CS2Gangs.GetGangsService().PushPlayerUpdate(inviteeGangPlayer);

        CS2Gangs.Logger.LogInformation($"{inviteeGangPlayer.PlayerName} has joined {gangName}");

        Server.NextFrame(() => {
            invitee.PrintLocalizedChat(CS2Gangs.GetBase().Localizer, "command_gangjoin_success", gangName);
        });

        return;
    }
}