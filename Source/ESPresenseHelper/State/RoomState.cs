using System.ComponentModel;
using ESPresenseHelper.Models;
using PropertyChanged.SourceGenerator;

namespace ESPresenseHelper.State;

/// <summary>
/// Uses nuget package for INotifyPropertyChanged
/// </summary>
public partial class RoomState : INotifyPropertyChanged
{
    public string Id { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [Notify]
    private SettingsState? _settings;

    [Notify]
    private DeviceState? _device;

    [Notify]
    private TelemetryModel? _telemetry;

    [Notify]
    private string? _status;

    [Notify]
    private bool _switch;

    [Notify]
    private bool _motion;

    [Notify]
    private bool _button;

    [Notify]
    private float _maxDistance;

    [Notify]
    private float _absorption;

    [Notify]
    private float _txRefRssi;

    [Notify]
    private float _rxAdjRssi;

    [Notify]
    private bool _arduinoOta;

    [Notify]
    private bool _autoUpdate;

    [Notify]
    private bool _prerelease;

    [Notify]
    private float _pirTimeout;

    [Notify]
    private float _radarTimeout;

    [Notify]
    private float _switch1Timeout;

    [Notify]
    private float _switch2Timeout;

    [Notify]
    private float _button1Timeout;

    [Notify]
    private float _button2Timeout;

    [Notify]
    private LedModel? _led1;

    public RoomState(string id)
    {
        Id = id;
    }
}
