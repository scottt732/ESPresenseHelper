using System.Diagnostics.Metrics;
using Sholo.Utils;

namespace ESPresenseHelper;

public static class MetersExtensions
{
    public static Counter<long> MqttBrokerConnected(this IMeters meters) => meters.GetOrAddInstrument<Counter<long>, long>(
        () => meters.Meter.CreateCounter<long>(
            name: "mqtt_broker_connected",
            description: "The number of times the MQTT broker connected"));

    public static Counter<long> MqttBrokerDisconnected(this IMeters meters) => meters.GetOrAddInstrument<Counter<long>, long>(
        () => meters.Meter.CreateCounter<long>(
            name: "mqtt_broker_connected",
            description: "The number of times the MQTT broker disconnected"));

    public static Counter<long> MqttBrokerConnectionFailure(this IMeters meters) => meters.GetOrAddInstrument<Counter<long>, long>(
        () => meters.Meter.CreateCounter<long>(
            name: "mqtt_broker_connection_failed",
            description: "The number of times the MQTT broker connection failed"));

    public static Counter<long> MqttBrokerSubscriptionSyncFailure(this IMeters meters) => meters.GetOrAddInstrument<Counter<long>, long>(
        () => meters.Meter.CreateCounter<long>(
            name: "mqtt_broker_subscription_sync_failed",
            description: "The number of times the MQTT broker subscription synchronization failed"));

    public static Counter<long> WebsocketConnected(this IMeters meters) => meters.GetOrAddInstrument<Counter<long>, long>(
        () => meters.Meter.CreateCounter<long>(
            name: "homeassistant_websocket_connected",
            description: "The number of times the Home Assistant websocket connected"));

    public static Counter<long> WebsocketRecovered(this IMeters meters) => meters.GetOrAddInstrument<Counter<long>, long>(
        () => meters.Meter.CreateCounter<long>(
            name: "homeassistant_websocket_recovered",
            description: "The number of times the Home Assistant websocket connection recovered"));

    public static Counter<long> StateChanged(this IMeters meters) => meters.GetOrAddInstrument<Counter<long>, long>(
        () => meters.Meter.CreateCounter<long>(
            name: "homeassistant_state_change",
            description: "The number of times the Home Assistant state changed for a registered sensor"));
}
