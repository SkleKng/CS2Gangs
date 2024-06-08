using api.plugin.models;
using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

public interface ICreditService
{
    public void DepositInBank(CCSPlayerController player, GangPlayer gangPlayer, Gang gang, int amount);
}