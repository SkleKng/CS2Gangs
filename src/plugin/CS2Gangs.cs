using api.plugin;
using api.plugin.services;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using plugin.commands;
using plugin.listeners;
using plugin.services;

namespace plugin;

public class CS2Gangs : BasePlugin, ICS2Gangs
{
    private IGangsService? database;
    public override string ModuleName => "CS2Gangs";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "EdgeGamers";
    public override string ModuleDescription => "Gangs for CS2 JB";
    
    private readonly Dictionary<string, Command> commands = new();

    public IGangsService GetGangsService()
    {
        return database!;
    }

    public BasePlugin GetBase()
    {
        return this;
    }

    public CS2GangsConfig? Config { get; set; }

    public void OnConfigParsed(CS2GangsConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        _ = new JoinListener(this);
        // _ = new ChatListener(this);
        database = new GangsService(this);
        Logger.LogInformation("gangs Loaded!!!");
        
        loadCommands();
    }

    private void loadCommands()
    {
        commands.Add("css_gangs", new GangsCmd(this));
        commands.Add("css_gang", new GangsCmd(this));
        commands.Add("css_gangcreate", new GangCreateCmd(this));
        commands.Add("css_gangleave", new GangLeaveCmd(this));
        commands.Add("css_gangdisband", new GangDisbandCmd(this));

        commands.Add("css_credits", new CreditsCmd(this));

        // Debug commands
        commands.Add("css_gangdebug", new GangDebugCmd(this));
        commands.Add("css_setcredits", new SetCreditsCmd(this));

        
        foreach (var command in commands)
        {
            AddCommand(command.Key, command.Value.Description ?? "No description provided", command.Value.OnCommand);
        }
    }
}