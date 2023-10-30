using System.ComponentModel;
using ESPresenseHelper.Models;
using PropertyChanged.SourceGenerator;

namespace ESPresenseHelper.State;

/// <summary>
/// Using nuget package for state
/// </summary>
public partial class DeviceReadingState : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public RoomState Room { get; }
    public SettingsState? RoomSettings => Room.Settings;
    public DeviceState? RoomDevice => Room.Device;
    public TelemetryModel? RoomTelemetry => Room.Telemetry;

    [Notify(Setter.Private)]
    private string _id = null!;

    [Notify(Setter.Private)]
    private int _idType;

    [Notify(Setter.Private)]
    private int _rssi1M;

    [Notify(Setter.Private)]
    private int _rssi;

    [Notify(Setter.Private)]
    private float _raw;

    [Notify(Setter.Private)]
    private float _distance;

    [Notify(Setter.Private)]
    private float? _speed;

    [Notify(Setter.Private)]
    private string _mac = null!;

    [Notify(Setter.Private)]
    private int _interval;

    public DeviceReadingState(RoomState room)
    {
        Room = room;
    }

    public override string ToString()
    {
        return $"{Distance}m from {Room.Id}";
    }

    public bool UpdateValues(DeviceReadingModel deviceReading)
    {
        if (deviceReading.Id != Id && Id != null)
        {
            throw new ArgumentException("X", nameof(deviceReading));
        }

        var anyChanged = false;

        if (deviceReading.IdType != IdType)
        {
            IdType = deviceReading.IdType;
            anyChanged = true;
        }

        if (deviceReading.Rssi1M != Rssi1M)
        {
            Rssi1M = deviceReading.Rssi1M;
            anyChanged = true;
        }

        if (deviceReading.Rssi != Rssi)
        {
            Rssi = deviceReading.Rssi;
            anyChanged = true;
        }

        if (Math.Abs(deviceReading.Raw - Raw) > float.Epsilon)
        {
            Raw = deviceReading.Raw;
            anyChanged = true;
        }

        if (Math.Abs(deviceReading.Distance - Distance) > float.Epsilon)
        {
            Distance = deviceReading.Distance;
            anyChanged = true;
        }

        if (
            (!deviceReading.Speed.HasValue && Speed.HasValue) ||
            (deviceReading.Speed.HasValue && !Speed.HasValue) ||
            (deviceReading.Speed.HasValue && Speed.HasValue && Math.Abs(deviceReading.Speed.Value - Speed.Value) > float.Epsilon))
        {
            Distance = deviceReading.Distance;
            anyChanged = true;
        }

        if (deviceReading.Mac != Mac)
        {
            Mac = deviceReading.Mac;
            anyChanged = true;
        }

        if (deviceReading.Interval != Interval)
        {
            Interval = deviceReading.Interval;
            anyChanged = true;
        }

        return anyChanged;
    }
}
