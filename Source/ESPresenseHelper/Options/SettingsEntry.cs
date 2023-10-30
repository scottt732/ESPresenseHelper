namespace ESPresenseHelper.Options;

[PublicAPI]
public class SettingsEntry
{
    public string DeviceId { get; set; } = null!;
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Icon { get; set; }

    public SettingsEntry()
    {
    }

    protected SettingsEntry(SettingsEntry other)
    {
        DeviceId = other.DeviceId;
        Id = other.Id;
        Name = other.Name;
        Icon = other.Icon;
    }
}
