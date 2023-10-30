using ESPresenseHelper.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Extensions.ManagedClient;

namespace ESPresenseHelper.Services;

public class DeviceMonitorService : BackgroundService
{
    private IManagedMqttClient MqttClient { get; }
    private IOptions<MonitorOptions> MonitorOptions { get; }
    private ILogger<DeviceMonitorService> Logger { get; }

    public DeviceMonitorService(
        IManagedMqttClient mqttClient,
        IOptions<MonitorOptions> monitorOptions,
        ILogger<DeviceMonitorService> logger
    )
    {
        MqttClient = mqttClient;
        MonitorOptions = monitorOptions;
        Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MqttClient.SubscribeAsync(MonitorOptions.Value.HelperMqttTopicRoot);

        Logger.LogInformation("Nodes:");
        foreach (var x in MonitorOptions.Value.Nodes)
        {
            Logger.LogInformation(" - {Name} ({Id})", x.Name, x.Id);
        }

        Logger.LogInformation("Enrolled Devices:");
        foreach (var x in MonitorOptions.Value.EnrolledDevices)
        {
            Logger.LogInformation(" - {Name} ({Id})", x.Name, x.Id);
        }

        await Task.Delay(TimeSpan.Zero, stoppingToken);
    }
}
