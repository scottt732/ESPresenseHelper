using Newtonsoft.Json;

namespace ESPresenseHelper.Models;

public class TelemetryModel
{
    [JsonProperty("ip")]
    public string Ip { get; set; } = null!;

    [JsonProperty("uptime")]
    public ulong Uptime { get; set; }

    [JsonProperty("firm")]
    public string FirmwareFlavor { get; set; } = null!;

    [JsonProperty("rssi")]
    public int Rssi { get; set; }

    [JsonProperty("ver")]
    public string Version { get; set; } = null!;

    [JsonProperty("adverts")]
    public ulong Advertisements { get; set; }

    [JsonProperty("seen")]
    public ulong Seen { get; set; }

    [JsonProperty("reported")]
    public ulong Reported { get; set; }

    [JsonProperty("freeHeap")]
    public ulong FreeHeap { get; set; }

    [JsonProperty("maxAllocHeap")]
    public ulong MaxAllocatedHeap { get; set; }

    [JsonProperty("memFrag")]
    public float MemoryFragmentation { get; set; }

    [JsonProperty("scanHighWater")]
    public ulong ScanHighWater { get; set; }
}
