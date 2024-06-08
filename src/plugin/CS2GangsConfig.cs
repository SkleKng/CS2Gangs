using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace plugin;

public class CS2GangsConfig : BasePluginConfig
{
    [JsonPropertyName("DBConnectionString")]
    public string? DBConnectionString { get; set; }

    [JsonPropertyName("DebugPermission")]
    public string? DebugPermission { get; set; }


    [JsonPropertyName("GangsCreationPrice")]
    public int GangCreationPrice { get; set; }

    [JsonPropertyName("GangInviteExpireMinutes")]
    public int GangInviteExpireMinutes { get; set; }

    [JsonPropertyName("DefaultPassiveCreditAmount")]
    public int DefaultPassiveCreditAmount { get; set; }

    [JsonPropertyName("VIPTier1PassiveCreditAmount")]
    public int VIPTier1PassiveCreditAmount { get; set; }

    [JsonPropertyName("VIPTier1Group")]
    public string? VIPTier1Group { get; set; }

    [JsonPropertyName("VIPTier2PassiveCreditAmount")]
    public int VIPTier2PassiveCreditAmount { get; set; }

    [JsonPropertyName("VIPTier2Group")]
    public string? VIPTier2Group { get; set; }

    [JsonPropertyName("VIPTier3PassiveCreditAmount")]
    public int VIPTier3PassiveCreditAmount { get; set; }

    [JsonPropertyName("VIPTier3Group")]
    public string? VIPTier3Group { get; set; }

    [JsonPropertyName("VIPTier4PassiveCreditAmount")]
    public int VIPTier4PassiveCreditAmount { get; set; }

    [JsonPropertyName("VIPTier4Group")]
    public string? VIPTier4Group { get; set; }
    
    [JsonPropertyName("CreditsDeliveryInterval")]
    public int CreditsDeliveryInterval { get; set; }
    [JsonPropertyName("GangChatCost")]
    public int GangChatCost { get; set; }

    [JsonPropertyName("GangExpandInitialCost")]
    public int GangExpandInitialCost { get; set; }

    [JsonPropertyName("GangExpandCostPerLevel")]
    public int GangExpandCostPerLevel { get; set; }

    [JsonPropertyName("MaxGangSize")]
    public int MaxGangSize { get; set; }

    [JsonPropertyName("InitialGangSize")]
    public int InitialGangSize { get; set; }
}