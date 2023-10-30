using ESPresenseHelper.Options;
using Microsoft.Extensions.Options;

namespace ESPresenseHelper.Settings;

[PublicAPI]
public class MonitorSettings
{
    public IDictionary<string, SettingsEntry> NodeConfigurationsById { get; }
    public IDictionary<string, SettingsEntry> NodeConfigurationsByDeviceId { get; }
    public IDictionary<string, SettingsEntry> EnrolledDeviceConfigurationsById { get; }
    public IDictionary<string, SettingsEntry> EnrolledDeviceConfigurationsByDeviceId { get; }

    public MonitorSettings(IOptions<MonitorOptions> settings)
    {
        var options = settings.Value;

        NodeConfigurationsByDeviceId = options.Nodes
            .Where(x => x.DeviceId != null!)
            .ToDictionary(
                x => x.DeviceId,
                x => x
            );

        EnrolledDeviceConfigurationsByDeviceId = options.EnrolledDevices
            .Where(x => x.DeviceId != null!)
            .ToDictionary(
                x => x.DeviceId,
                x => x
            );

        NodeConfigurationsById = options.Nodes
            .Where(x => x.Id != null)
            .GroupBy(x => x.Id!)
            .ToDictionary(
                x => x.Key,
                x => x.Single()
            );

        EnrolledDeviceConfigurationsById = options.EnrolledDevices
            .Where(x => x.Id != null)
            .GroupBy(x => x.Id!)
            .ToDictionary(
                x => x.Key,
                x => x.Single()
            );
    }

    public bool TryGetNodeConfigurationById(string id, out SettingsEntry? nodeConfiguration)
    {
        if (NodeConfigurationsById.TryGetValue(id, out nodeConfiguration))
        {
            return true;
        }

        if (id.StartsWith("node:", StringComparison.Ordinal) && NodeConfigurationsById.TryGetValue(id[5..], out nodeConfiguration))
        {
            return true;
        }

        nodeConfiguration = null;
        return false;
    }

    public bool TryGetNodeConfigurationByDeviceId(string deviceId, out SettingsEntry? nodeConfiguration)
    {
        if (NodeConfigurationsByDeviceId.TryGetValue(deviceId, out nodeConfiguration))
        {
            return true;
        }

        nodeConfiguration = null;
        return false;
    }

    public bool TryGetEnrolledDeviceConfigurationById(string id, out SettingsEntry? enrolledDeviceConfiguration)
    {
        if (EnrolledDeviceConfigurationsById.TryGetValue(id, out enrolledDeviceConfiguration))
        {
            return true;
        }

        enrolledDeviceConfiguration = null;
        return false;
    }

    public bool TryGetEnrolledDeviceConfigurationByDeviceId(string deviceId, out SettingsEntry? enrolledDeviceConfiguration)
    {
        if (EnrolledDeviceConfigurationsByDeviceId.TryGetValue(deviceId, out enrolledDeviceConfiguration))
        {
            return true;
        }

        enrolledDeviceConfiguration = null;
        return false;
    }
}
