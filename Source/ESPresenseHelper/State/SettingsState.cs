using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace ESPresenseHelper.State;

/// <summary>
/// Uses nuget package for INotifyPropertyChanged
/// </summary>
public partial class SettingsState : INotifyPropertyChanged
{
    public string DeviceId { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [Notify]
    private DeviceState? _device;

    [Notify]
    private RoomState? _room;

    [Notify]
    private string? _id;

    [Notify]
    private string? _desiredId;

    [Notify]
    private string? _desiredName;

    [Notify]
    private string? _name;

    [Notify]
    private string? _icon;

    public SettingsState(string deviceId)
    {
        DeviceId = deviceId;
    }
}
