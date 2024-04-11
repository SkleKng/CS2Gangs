using api.plugin.models;
using api.plugin.services;
using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace plugin.services;

public class GangsService : IGangsService
{
    private readonly CS2Gangs CS2Gangs;

    public GangsService(CS2Gangs CS2Gangs)
    {
        this.CS2Gangs = CS2Gangs;
        createTables();
    }

    public async Task<Gang?> GetGang(int gangid)
    {
        using (var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString))
        {
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<Gang>("SELECT * FROM cs2_gangs_gangs WHERE id = @gangid",
                new { gangid });
        }
    }

    public async Task<GangPlayer?> GetGangPlayer(ulong steamid)
    {
        using (var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString))
        {
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<GangPlayer>(
                "SELECT * FROM cs2_gangs_players WHERE steamid = @steamid", new { steamid });
        }
    }

    public async void UpdatePlayerOnJoin(ulong steamid, string playername)
    {
        if (!await playerExists(steamid))
            createNewGangPlayer(steamid, playername);
        else
            updateTimeStamp(steamid);
    }

    public async Task<IEnumerable<GangPlayer>> GetGangMembers(int gangid)
    {
        using var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString);
        conn.Open();

        return await conn.QueryAsync<GangPlayer>("SELECT * FROM cs2_gangs_players WHERE gangid = @gangid", new { gangid });
    }

    public async Task<bool> GangNameExists(string name)
    {
        using var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString);
        conn.Open();

        return await conn.ExecuteScalarAsync<bool>("SELECT COUNT(*) FROM cs2_gangs_gangs WHERE name = @name", new { name });
    }

    public async Task<int> GetNextGangId()
    {
        using var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString);
        conn.Open();

        string query = "SHOW TABLE STATUS LIKE 'cs2_gangs_gangs'";
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(query);
        return (int)result.Auto_increment;
    }

    public async void DisbandGang(Gang gang)
    {
        using var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString);
        conn.Open();

        await conn.ExecuteAsync("DELETE FROM cs2_gangs_gangs WHERE id = @Id", gang);
        await conn.ExecuteAsync("UPDATE cs2_gangs_players SET gangid = NULL, invitedby = NULL, gangrank = NULL WHERE gangid = @Id", gang);
    }

    public async void PushGangUpdate(Gang gang)
    {
        using var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString);
        conn.Open();

        await conn.ExecuteAsync(
            "INSERT INTO cs2_gangs_gangs (id, name, description, maxsize, credits, colors, colorpreference, chat, chatcolor, bombicons, emotes) VALUES (@Id, @Name, @description, @MaxSize, @Credits, @Colors, @ColorPreference, @Chat, @ChatColor, @BombIcons, @Emotes) ON DUPLICATE KEY UPDATE name = @Name, description = @description, maxsize = @MaxSize, credits = @Credits, colors = @Colors, colorpreference = @ColorPreference, chat = @Chat, chatcolor = @ChatColor, bombicons = @BombIcons, emotes = @Emotes",
            gang);
    }

    public async void PushPlayerUpdate(GangPlayer player)
    {
        using var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString);
        conn.Open();

        await conn.ExecuteAsync(
            "UPDATE cs2_gangs_players SET playername = @PlayerName, gangid = @GangId, gangrank = @GangRank, invitedby = @InvitedBy, credits = @Credits, monthlylr = @MonthlyLR, lifetimelr = @LifetimeLR, monthlyctkills = @MonthlyCTKills, lifetimectkills = @LifetimeCTKills, monthlytkills = @MonthlyTKills, lifetimetkills = @LifetimeTKills, monthlyrebelkills = @MonthlyRebelKills, lifetimerebelkills = @LifetimeRebelKills, lastjoin = CURRENT_TIMESTAMP WHERE steamid = @SteamId",
            player);
    }

    private async Task<bool> playerExists(ulong steamid)
    {
        using (var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString))
        {
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM cs2_gangs_players WHERE steamid = @steamid";
            cmd.Parameters.AddWithValue("@steamid", steamid);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }

    private async void createNewGangPlayer(ulong steamid, string playername)
    {
        using (var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString))
        {
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO cs2_gangs_players (steamid, playername) VALUES (@steamid, @playername)";
            cmd.Parameters.AddWithValue("@steamid", steamid);
            cmd.Parameters.AddWithValue("@playername", playername);
            cmd.ExecuteNonQuery();
        }
    }

    private async void updateTimeStamp(ulong steamid)
    {
        using (var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString))
        {
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE cs2_gangs_players SET lastjoin = CURRENT_TIMESTAMP WHERE steamid = @steamid";
            cmd.Parameters.AddWithValue("@steamid", steamid);
            cmd.ExecuteNonQuery();
        }
    }

    private async void createTables()
    {
        CS2Gangs.GetBase().Logger.LogInformation($"Attempting to establish connection using the following connection string: {CS2Gangs.Config!.DBConnectionString}");

        using (var conn = new MySqlConnection(CS2Gangs.Config!.DBConnectionString))
        {
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                              CREATE TABLE IF NOT EXISTS cs2_gangs_gangs (
                                  id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                                  name VARCHAR(255) NOT NULL UNIQUE,
                                  description VARCHAR(255) DEFAULT NULL,
                                  maxsize INT NOT NULL DEFAULT 10,
                                  credits INT NOT NULL DEFAULT 0,
                                  colors INT NOT NULL DEFAULT 0,
                                  colorpreference INT NOT NULL DEFAULT 0,
                                  chat BOOL NOT NULL DEFAULT FALSE,
                                  chatcolor CHAR DEFAULT NULL,
                                  bombicons INT NOT NULL DEFAULT 0,
                                  emotes INT NOT NULL DEFAULT 0
                              )
                              """;
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = """
                              CREATE TABLE IF NOT EXISTS cs2_gangs_players (
                                  steamid BIGINT NOT NULL PRIMARY KEY,
                                  playername VARCHAR(255),
                                  gangid INT DEFAULT NULL,
                                  gangrank INT DEFAULT NULL,
                                  invitedby VARCHAR(255) DEFAULT NULL,
                                  credits INT NOT NULL DEFAULT 0,
                                  monthlylr INT NOT NULL DEFAULT 0,
                                  lifetimelr INT NOT NULL DEFAULT 0,
                                  monthlyctkills INT NOT NULL DEFAULT 0,
                                  lifetimectkills INT NOT NULL DEFAULT 0,
                                  monthlytkills INT NOT NULL DEFAULT 0,
                                  lifetimetkills INT NOT NULL DEFAULT 0,
                                  monthlyrebelkills INT NOT NULL DEFAULT 0,
                                  lifetimerebelkills INT NOT NULL DEFAULT 0,
                                  lastjoin TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                              )
                              """;

            await cmd.ExecuteNonQueryAsync();
        }
    }
}