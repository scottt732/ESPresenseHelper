using ESPresenseHelper.Models;
using ESPresenseHelper.Options;
using ESPresenseHelper.Settings;
using Sholo.Utils.Collections;

namespace ESPresenseHelper.State;

public class ESPresenseState
{
    private LazyConcurrentDictionaryThing<string, RoomState> RoomsById { get; } = new();
    private LazyConcurrentDictionaryThing<string, SettingsState> SettingsByDeviceId { get; } = new();
    private LazyConcurrentDictionaryThing<string, DeviceState> DevicesById { get; } = new();

    private MonitorSettings MonitorSettings { get; }

    public ESPresenseState(MonitorSettings monitorSettings)
    {
        MonitorSettings = monitorSettings;
    }

    public IDictionary<string, RoomState> GetRoomsById()
    {
        return RoomsById
            .ToArray() // snapshot
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public IDictionary<string, SettingsState> GetSettingsByDeviceId()
    {
        return SettingsByDeviceId
            .ToArray() // snapshot
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public IDictionary<string, DeviceState> GetDevicesById()
    {
        return DevicesById
            .ToArray() // snapshot
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public RoomState GetOrCreateRoomById(string id)
    {
        var room = RoomsById.GetOrAdd(id, () => new RoomState(id));

        if (room.Settings == null)
        {
            var settings = GetSettingsById(id);

            if (settings != null)
            {
                LinkRoomToSettings(room, settings);
            }
        }

        if (room.Device == null)
        {
            if (DevicesById.TryGetValue(id, out var device))
            {
                LinkRoomToDevice(room, device);
            }
        }

        return room;
    }

    public SettingsState GetOrCreateSettingsByDeviceId(string deviceId)
    {
        var settings = SettingsByDeviceId.GetOrAdd(deviceId, () =>
        {
            var settings = new SettingsState(deviceId);

            // Since config is static (for now), init this once here.
            if (MonitorSettings.TryGetEnrolledDeviceConfigurationByDeviceId(settings.DeviceId, out var configuration))
            {
                LinkConfigurationToSettings(settings, configuration!);
            }

            return settings;
        });

        if (settings.Room == null && settings.Id != null)
        {
            if (RoomsById.TryGetValue(settings.Id, out var roomState))
            {
                LinkRoomToSettings(roomState, settings);
            }
        }

        if (settings.Device == null && settings.Id != null)
        {
            if (DevicesById.TryGetValue(settings.Id, out var device))
            {
                LinkSettingsToDevice(settings, device);
            }
        }

        return settings;
    }

    public DeviceState GetOrCreateDeviceStateById(string id)
    {
        var device = DevicesById.GetOrAdd(id, () => new DeviceState(id));

        if (device.Room == null && device.Id.StartsWith("node:", StringComparison.Ordinal))
        {
            if (RoomsById.TryGetValue(device.Id.Substring(5), out var room))
            {
                LinkRoomToDevice(room, device);
            }
        }

        if (device.Settings == null)
        {
            var settings = GetSettingsById(id);

            if (settings != null)
            {
                LinkSettingsToDevice(settings, device);
            }
        }

        return device;
    }

    public DeviceState? RecordReading(string id, string roomId, DeviceReadingModel deviceReading)
    {
        var deviceState = GetOrCreateDeviceStateById(id);
        return RecordReading(roomId, deviceState, deviceReading);
    }

    private SettingsState? GetSettingsById(string id)
    {
        var settings = SettingsByDeviceId.ToArray() // snapshot
            .Where(x => x.Value.Id == id)
            .Select(x => x.Value)
            .SingleOrDefault();
        return settings;
    }

    private void LinkRoomToSettings(RoomState room, SettingsState settings)
    {
        room.Settings = settings;
        settings.Room = room;
    }

    private void LinkRoomToDevice(RoomState room, DeviceState device)
    {
        room.Device = device;
        device.Room = room;
    }

    private void LinkSettingsToDevice(SettingsState settings, DeviceState device)
    {
        settings.Device = device;
        device.Settings = settings;
    }

    private void LinkConfigurationToSettings(SettingsState settings, SettingsEntry settingsEntry)
    {
        if (settingsEntry.Id != null && settingsEntry.Id != settings.Id)
        {
            settings.DesiredId = settingsEntry.Id;
        }

        if (settingsEntry.Name != null && settingsEntry.Name != settings.Name)
        {
            settings.DesiredName = settingsEntry.Name;
        }

        settings.Icon = settingsEntry.Icon;
    }

    private DeviceState? RecordReading(string roomId, DeviceState deviceState, DeviceReadingModel deviceReading)
    {
        var roomState = GetOrCreateRoomById(roomId);
        var deviceReadingState = deviceState.GetOrCreateDeviceReadingState(roomState);

        if (deviceReadingState.UpdateValues(deviceReading))
        {
            var deviceInRoomId = deviceState.RecalculateLocation();
            if (deviceInRoomId != null)
            {
                var deviceInRoomState = GetOrCreateRoomById(deviceInRoomId);
                deviceState.Room = deviceInRoomState;
                return deviceState;
            }
            else
            {
                deviceState.Room = null;
            }
        }

        deviceState.Room = null;
        return null;
    }
}
