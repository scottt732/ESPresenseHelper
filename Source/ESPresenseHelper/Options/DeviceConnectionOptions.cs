namespace ESPresenseHelper.Options;

[PublicAPI]
public class DeviceConnectionOptions
{
    public string ConnectionType { get; set; } = null!;
    public string ConnectionIdentifier { get; set; } = null!;
}
