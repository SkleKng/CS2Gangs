using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace plugin;

public class CS2GangsConfig : BasePluginConfig
{
    [JsonPropertyName("ClientPrefsDBConnectionString")]
    public string? ClientPrefsDBConnectionString { get; set; }

    [JsonPropertyName("JailbreakDBConnectionString")]
    public string? JailbreakDBConnectionString { get; set; }
    [JsonPropertyName("DebugPermission")]
    public string? DebugPermission { get; set; }

    [JsonPropertyName("GangsCreationPrice")]
    public int GangCreationPrice { get; set; }
}