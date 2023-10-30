using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ESPresenseHelper.Models;

[PublicAPI]
public class DeviceReadingModel
{
    [Required]
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("idType")]
    [Range(1, int.MaxValue)]
    public int IdType { get; set; }

    [JsonProperty("rssi@1m")]
    [Range(int.MinValue, -1)]
    public int Rssi1M { get; set; }

    [JsonProperty("rssi")]
    [Range(int.MinValue, -1)]
    public int Rssi { get; set; }

    [JsonProperty("raw")]
    [Range(float.Epsilon, float.MaxValue)]
    public float Raw { get; set; }

    [JsonProperty("distance")]
    [Range(float.Epsilon, float.MaxValue)]
    public float Distance { get; set; }

    [JsonProperty("speed")]
    [Required]
    public float? Speed { get; set; }

    [Required]
    [JsonProperty("mac")]
    public string Mac { get; set; } = null!;

    [JsonProperty("interval")]
    [Range(1, int.MaxValue)]
    public int Interval { get; set; }
}
