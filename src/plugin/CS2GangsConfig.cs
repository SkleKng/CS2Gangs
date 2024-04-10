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
}