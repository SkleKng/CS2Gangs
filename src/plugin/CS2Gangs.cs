using api.plugin;
using api.plugin.models;
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
    private IGangInviteService? inviteService;
    public override string ModuleName => "CS2Gangs";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "EdgeGamers";
    public override string ModuleDescription => "Gangs for CS2 JB";
    
    private readonly Dictionary<string, Command> commands = new();

    public IGangsService GetGangsService()
    {
        return database!;
    }

    public IGangInviteService GetGangInviteService()
    {
        return inviteService!;
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
        database = new GangsService(this);
        inviteService = new GangInviteService(this);
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

        //Member shit
        commands.Add("css_gangmembers", new GangMembersCmd(this));
        commands.Add("css_gangmember", new GangMemberCmd(this));
        commands.Add("css_gangpromote", new GangPromoteCmd(this));
        commands.Add("css_gangdemote", new GangDemoteCmd(this));
        commands.Add("css_gangkick", new GangKickCmd(this));
        commands.Add("css_gangtransfer", new GangTransferCmd(this));
        commands.Add("css_ganginvite", new GangInviteCommand(this));
        commands.Add("css_gangjoin", new GangJoinCommand(this));

        // Debug commands
        commands.Add("css_gangdebug", new GangDebugCmd(this));
        commands.Add("css_setcredits", new SetCreditsCmd(this));

        
        foreach (var command in commands)
        {
            AddCommand(command.Key, command.Value.Description ?? "No description provided", command.Value.OnCommand);
        }
    }
}

//TODO Add gang chat which entails the following - { credit gain methods (DS MULTI), gang credits, gang perk system, gang chat, log for admins } add announcer service with methods to announce to gang (promote, demote, kick) and announce to server (create, disband)