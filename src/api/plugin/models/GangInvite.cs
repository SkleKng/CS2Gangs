using api.plugin.models;

public class GangInvite {
    public long inviterId { get; set; }
    public string inviterName { get; set; }
    public long inviteeId { get; set; }
    public string inviteeName { get; set; }
    public int gangId { get; set; }
    public string gangName { get; set; }
    public DateTime inviteTime { get; set; }

    public GangInvite(long inviterId, string inviterName, long inviteeId, string inviteeName, int gangId, string gangName, DateTime inviteTime) {
        this.inviterId = inviterId;
        this.inviterName = inviterName;
        this.inviteeId = inviteeId;
        this.inviteeName = inviteeName;
        this.gangId = gangId;
        this.gangName = gangName;
        this.inviteTime = inviteTime;
    }
}