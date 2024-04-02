using api.plugin.models;

namespace api.plugin.services;

public interface IGangsService
{
    Task<Gang?> GetGang(int gangid);
    Task<GangPlayer?> GetGangPlayer(ulong steamid);
    void UpdatePlayerOnJoin(ulong steamid, string playername);
    Task<IEnumerable<GangPlayer>> GetGangMembers(int gangid);
    Task<bool> GangNameExists(string name);
    Task<int> GetNextGangId();
    void DisbandGang(Gang gang);
    void PushGangUpdate(Gang gang);
    void PushPlayerUpdate(GangPlayer player);
}