using ESPresenseHelper.Models;
using ESPresenseHelper.State;
using Microsoft.Extensions.Logging;
using Sholo.Mqtt.Controllers;

namespace ESPresenseHelper.Controllers;

public class SettingsController : MqttControllerBase
{
    private ESPresenseState State { get; }
    private ILogger Logger { get; }

    public SettingsController(
        ESPresenseState state,
        ILogger<SettingsController> logger
    )
    {
        State = state;
        Logger = logger;
    }

    [Topic("espresense/settings/+deviceId/config", "Settings")]
    public async Task<bool> SettingsAsync(
        string deviceId,
        SettingsModel settings,
        CancellationToken cancellationToken)
    {
        var isNode = settings.Id.StartsWith("node:", StringComparison.Ordinal);

        var deviceSettings = State.GetOrCreateSettingsByDeviceId(deviceId);
        deviceSettings.Id = settings.Id;
        deviceSettings.Name = settings.Name;

        if ((!string.IsNullOrEmpty(deviceSettings.DesiredId) && deviceSettings.Id != deviceSettings.DesiredId) ||
            (!string.IsNullOrEmpty(deviceSettings.DesiredName) && deviceSettings.Name != deviceSettings.DesiredName))
        {
            await TrySetDeviceIdAndName(
                deviceSettings.DeviceId,
                deviceSettings.DesiredId ?? deviceSettings.Id,
                deviceSettings.DesiredName ?? deviceSettings.Name,
                cancellationToken
            );
        }

        return true;
    }

    private Task TrySetDeviceIdAndName(string deviceId, string? desiredId, string? desiredName, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Attempting to set id to {Id}, name to {Name} for {DeviceId}", desiredId, desiredName, deviceId);
        return Task.CompletedTask;
    }
}
