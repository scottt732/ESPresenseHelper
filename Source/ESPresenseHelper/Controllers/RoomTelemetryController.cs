using System.Diagnostics.Metrics;
using ESPresenseHelper.Models;
using Microsoft.Extensions.Logging;
using Sholo.Mqtt.Controllers;
using Sholo.Utils;

namespace ESPresenseHelper.Controllers;

public class RoomTelemetryController
{
    private IMeters Meters { get; }
    private ILogger Logger { get; }

    private Counter<ulong> UptimeCounter { get; }
    private UpDownCounter<int> RssiCounter { get; }
    private Counter<ulong> AdvertisementsCounter { get; }
    private Counter<ulong> SeenCounter { get; }
    private Counter<ulong> AdvertsCounter { get; }
    private Counter<ulong> ReportedCounter { get; }
    private UpDownCounter<ulong> FreeHeapCounter { get; }
    private UpDownCounter<ulong> MaxHeapCounter { get; }
    private UpDownCounter<ulong> ScanStackCounter { get; }
    private UpDownCounter<ulong> LoopStackCounter { get; }

    public RoomTelemetryController(
        IMeters meters,
        ILogger<RoomTelemetryController> logger
    )
    {
        Meters = meters;
        Logger = logger;

        UptimeCounter = Meters.Meter.CreateCounter<ulong>("espresence.room.uptime", "seconds", "Node uptime");
        RssiCounter = Meters.Meter.CreateUpDownCounter<int>("espresence.room.rssi", "mW", "Received Signal Strength Indicator");
        AdvertisementsCounter = Meters.Meter.CreateCounter<ulong>("espresence.room.uptime", "seconds", "Node uptime");
        SeenCounter = Meters.Meter.CreateCounter<ulong>("espresence.room.seen", null, "Advertisements seen from other devices");
        AdvertsCounter = Meters.Meter.CreateCounter<ulong>("espresence.room.adverts", null, "Advertisements broadcast");
        ReportedCounter = Meters.Meter.CreateCounter<ulong>("espresence.room.reported", null, "?");
        FreeHeapCounter = Meters.Meter.CreateUpDownCounter<ulong>("espresence.room.free_heap", null, "Free heap size");
        MaxHeapCounter = Meters.Meter.CreateUpDownCounter<ulong>("espresence.room.max_heap", null, "Max heap size");
        ScanStackCounter = Meters.Meter.CreateUpDownCounter<ulong>("espresence.room.scan_stack", null, "?");
        LoopStackCounter = Meters.Meter.CreateUpDownCounter<ulong>("espresence.room.loop_stack", null, "?");
    }

    [Topic("espresense/rooms/+id/telemetry")]
    public Task<bool> TelemetryAsync(IMeters meters, string id, TelemetryModel telemetry)
    {
        Logger.LogDebug("Room {Id} telemetry: {Telemtry}", id, telemetry);

        // TODO: need to use observable gauges for things like up time and metrics where raw values aren't additive

        return Task.FromResult(true);
    }
}
