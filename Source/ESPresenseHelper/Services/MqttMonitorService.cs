using System.Diagnostics.Metrics;
using ESPresenseHelper.HealthChecks;
using Microsoft.Extensions.Hosting;
using MQTTnet.Extensions.ManagedClient;
using Sholo.Utils;

namespace ESPresenseHelper.Services;

public class MqttMonitorService : BackgroundService
{
    private IManagedMqttClient MqttClient { get; }
    private ReadinessCheck ReadinessCheck { get; }
    private IMeters Meters { get; }
    private bool Initialized { get; set; }

    private Counter<long> MqttBrokerConnected { get; }
    private Counter<long> MqttBrokerConnectionFailure { get; }
    private Counter<long> MqttBrokerDisconnected { get; }
    private Counter<long> MqttBrokerSubscriptSyncFailure { get; }

    public MqttMonitorService(
        IManagedMqttClient mqttClient,
        ReadinessCheck readinessCheck,
        IMeters meters)
    {
        MqttClient = mqttClient;
        ReadinessCheck = readinessCheck;
        Meters = meters;

        MqttBrokerConnected = Meters.MqttBrokerConnected();
        MqttBrokerConnectionFailure = Meters.MqttBrokerConnectionFailure();
        MqttBrokerDisconnected = Meters.MqttBrokerDisconnected();
        MqttBrokerSubscriptSyncFailure = Meters.MqttBrokerSubscriptionSyncFailure();
    }

    public override void Dispose()
    {
        if (Initialized)
        {
            MqttClient.ConnectedAsync -= ConnectedAsync;
            MqttClient.DisconnectedAsync -= DisconnectedAsync;
            MqttClient.ConnectingFailedAsync -= ConnectingFailedAsync;
            MqttClient.SynchronizingSubscriptionsFailedAsync -= SynchronizingSubscriptionsFailedAsync;
        }

        GC.SuppressFinalize(this);
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MqttClient.ConnectedAsync += ConnectedAsync;
        MqttClient.DisconnectedAsync += DisconnectedAsync;
        MqttClient.ConnectingFailedAsync += ConnectingFailedAsync;
        MqttClient.SynchronizingSubscriptionsFailedAsync += SynchronizingSubscriptionsFailedAsync;
        Initialized = true;

        var blockUntilStopRequested = new SemaphoreSlim(0);

        // ReSharper disable once AccessToDisposedClosure
        stoppingToken.Register(() => blockUntilStopRequested.Release(1));

        if (MqttClient.IsConnected)
        {
            await ConnectedAsync(null);
        }

        await blockUntilStopRequested.WaitAsync(CancellationToken.None);

        blockUntilStopRequested.Dispose();
    }

    private Task ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs? arg)
    {
        MqttBrokerConnected.Add(1);
        ReadinessCheck.MqttBrokerConnected = true;
        return Task.CompletedTask;
    }

    private Task DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
    {
        MqttBrokerDisconnected.Add(1);
        ReadinessCheck.MqttBrokerConnected = false;
        return Task.CompletedTask;
    }

    private Task ConnectingFailedAsync(ConnectingFailedEventArgs arg)
    {
        MqttBrokerConnectionFailure.Add(1);
        ReadinessCheck.MqttBrokerConnected = false;
        return Task.CompletedTask;
    }

    private Task SynchronizingSubscriptionsFailedAsync(ManagedProcessFailedEventArgs arg)
    {
        MqttBrokerSubscriptSyncFailure.Add(1);
        ReadinessCheck.MqttBrokerConnected = false;
        return Task.CompletedTask;
    }
}
