using api.plugin.models;
using api.plugin.services;
using CounterStrikeSharp.API.Core;
using plugin;

namespace api.plugin;

public interface ICS2Gangs : IPluginConfig<CS2GangsConfig>
{
    public IGangsService GetGangsService();
    public IGangInviteService GetGangInviteService();
    BasePlugin GetBase();
}