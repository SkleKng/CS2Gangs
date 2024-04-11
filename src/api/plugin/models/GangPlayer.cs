namespace api.plugin.models;

public class GangPlayer(
    long steamId,
    string? playerName,
    int? gangId,
    int? gangRank,
    string? invitedBy,
    int credits,
    int monthlyLR,
    int lifetimeLR,
    int monthlyCTKills,
    int lifetimeCTKills,
    int monthlyTKills,
    int lifetimeTKills,
    int monthlyRebelKills,
    int lifetimeRebelKills,
    DateTime lastJoin)
{
    public long SteamId { get; } = steamId;
    public string? PlayerName { get; set; } = playerName;
    public int? GangId { get; set; } = gangId;
    public int? GangRank { get; set; } = gangRank;
    public string? InvitedBy { get; set; } = invitedBy;
    public int Credits { get; set; } = credits;
    public int MonthlyLR { get; set; } = monthlyLR;
    public int LifetimeLR { get; set; } = lifetimeLR;
    public int MonthlyCTKills { get; set; } = monthlyCTKills;
    public int LifetimeCTKills { get; set; } = lifetimeCTKills;
    public int MonthlyTKills { get; set; } = monthlyTKills;
    public int LifetimeTKills { get; set; } = lifetimeTKills;
    public int MonthlyRebelKills { get; set; } = monthlyRebelKills;
    public int LifetimeRebelKills { get; set; } = lifetimeRebelKills;
    public DateTime LastJoin { get; set; } = lastJoin;

    public override string ToString()
    {
        return $"SteamId: {SteamId}, PlayerName: {PlayerName}, GangId: {GangId}, GangRank: {GangRank}, InvitedBy: {InvitedBy}, Credits: {Credits}, MonthlyLR: {MonthlyLR}, LifetimeLR: {LifetimeLR}, MonthlyCTKills: {MonthlyCTKills}, LifetimeCTKills: {LifetimeCTKills}, MonthlyTKills: {MonthlyTKills}, LifetimeTKills: {LifetimeTKills}, MonthlyRebelKills: {MonthlyRebelKills}, LifetimeRebelKills: {LifetimeRebelKills}, LastJoin: {LastJoin}";
    }
}