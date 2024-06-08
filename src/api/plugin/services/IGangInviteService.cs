using api.plugin.models;
using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

public interface IGangInviteService
{
    void SendInvite(CCSPlayerController sender, CCSPlayerController receiver, GangPlayer senderGangPlayer, GangPlayer receiverGangPlayer, Gang gang);
    void AcceptInvite(CCSPlayerController invitee, GangPlayer inviteeGangPlayer, string? gangName);
}