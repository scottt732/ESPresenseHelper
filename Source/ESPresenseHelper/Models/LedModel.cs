using Newtonsoft.Json;

namespace ESPresenseHelper.Models;

public class LedModel
{
    [JsonProperty("state")]
    public string State { get; set; } = null!;

    [JsonProperty("brightness")]
    public int Brightness { get; set; }

    [JsonProperty("color")]
    public RgbColor Color { get; set; }
}
