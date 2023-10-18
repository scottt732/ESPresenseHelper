namespace ESPresenseHelper.Options;

[PublicAPI]
public class DeviceOptions
{
    public string Identifier { get; set; } = null!;
    public string Manufacturer { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SwVersion { get; set; } = null!;
    public DeviceConnectionOptions Connection { get; set; } = new();
}
