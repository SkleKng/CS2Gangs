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
    private IAnnouncerService? announcerService;
    private ICreditService? creditService;
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

    public IAnnouncerService GetAnnouncerService()
    {
        return announcerService!;
    }

    public ICreditService GetCreditService()
    {
        return creditService!;
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
        announcerService = new AnnouncerService(this);
        creditService = new CreditService(this);
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

        // Bank commands
        commands.Add("css_gangbank", new GangBankCmd(this));
        commands.Add("css_gangdeposit", new GangDepositCmd(this));
        commands.Add("css_gangperks", new GangPerksCmd(this));
        commands.Add("css_gangpurchase", new GangPurchaseCmd(this));

        commands.Add("css_gc", new GangChatCmd(this));
        commands.Add("css_gangchat", new GangChatCmd(this));

        
        foreach (var command in commands)
        {
            AddCommand(command.Key, command.Value.Description ?? "No description provided", command.Value.OnCommand);
        }
    }
}

// TODO: Add gang chat log for admins, add cache for GangPlayer, allow players to be targetted in member commands instead of just steamids, add chat listeners for create/gang chat