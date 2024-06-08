namespace api.plugin.models;

public class Gang(
    int id,
    string name,
    int maxSize,
    string description = "",
    int credits = 0,
    int colors = 0,
    int colorPreference = 0,
    bool chat = false,
    char? chatColor = null,
    int bombIcons = 0,
    int emotes = 0)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int MaxSize { get; set; } = maxSize;
    public string Description { get; set; } = description;
    public int Credits { get; set; } = credits;
    public int Colors { get; set; } = colors;
    public int ColorPreference { get; set; } = colorPreference;
    public bool Chat { get; set; } = chat;
    public char? ChatColor { get; set; } = chatColor;
    public int BombIcons { get; set; } = bombIcons;
    public int Emotes { get; set; } = emotes;

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, MaxSize: {MaxSize}, Description: {Description}, Credits: {Credits}, Colors: {Colors}, ColorPreference: {ColorPreference}, Chat: {Chat}, ChatColor: {ChatColor}, BombIcons: {BombIcons}, Emotes: {Emotes}";
    }
}