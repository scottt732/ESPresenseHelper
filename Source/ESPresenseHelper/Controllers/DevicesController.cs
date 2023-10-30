using ESPresenseHelper.Models;
using ESPresenseHelper.State;
using Microsoft.Extensions.Logging;
using Sholo.Mqtt.Controllers;

namespace ESPresenseHelper.Controllers;

public class DevicesController : MqttControllerBase
{
    private ESPresenseState State { get; }
    private ILogger Logger { get; }

    public DevicesController(
        ESPresenseState state,
        ILogger<DevicesController> logger)
    {
        State = state;
        Logger = logger;
    }

    [Topic("espresense/devices/+id")]
    public Task<bool> DeviceStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken)
    {
        var deviceState = State.GetOrCreateDeviceStateById(id);
        deviceState.Status = status;

        // TODO: Fire event?
        return Task.FromResult(true);
    }

    [Topic("espresense/devices/+id/+room")]
    public Task<bool> DeviceAsync(
        string id,
        string room,
        DeviceReadingModel deviceReading,
        CancellationToken cancellationToken)
    {
        if (id.StartsWith("md:ffff", StringComparison.Ordinal))
        {
            return Task.FromResult(true);
        }

        var deviceState = State.RecordReading(id, room, deviceReading);

        if (id.StartsWith("node:", StringComparison.Ordinal) && deviceState?.Room != null && deviceState?.ClosestNode?.Room != null)
        {
            Logger.LogDebug(
                "Room {RoomId} is {Distance}m from {Room}",
                id.Substring(5),
                deviceState.ClosestNode.Distance,
                deviceState.ClosestNode.Room.Id
            );
        }
        else if (deviceState?.ClosestNode?.Room != null)
        {
            Logger.LogInformation(
                "Id: {Id} is now close to {Rooms}",
                id,
                deviceState.ClosestNodes
            );
        }
        else if (deviceState is { ClosestNode: not null })
        {
            Logger.LogInformation(
                "Id: {Id} is now closest to {ClosestDevice} ({ClosestNodeId})",
                id,
                deviceState.ClosestNode.Mac,
                deviceState.ClosestNode.Id
            );
        }

        /*
        Logger.LogInformation(
            "Device: {DeviceId}, Node: {Node}, Disc: {Disc}, Distance: {Distance}, IdType: {IdType}, Interval: {Interval}, Mac: {Mac}, Raw: {Raw}, Rssi: {Rssi}, Rssi@1m: {Rssi1M}, Speed: {Speed}",
            device.DeviceId.RedactDeviceId(),
            node,
            device.Disc,
            device.Distance,
            device.IdType,
            device.Interval,
            device.Mac,
            device.Raw,
            device.Rssi,
            device.Rssi1M,
            device.Speed
        );
        */
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }
}
