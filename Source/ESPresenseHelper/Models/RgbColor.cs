using Newtonsoft.Json;

namespace ESPresenseHelper.Models;

public record struct RgbColor
{
    [JsonProperty("r")]
    public ushort Red { get; set; }

    [JsonProperty("g")]
    public ushort Green { get; set; }

    [JsonProperty("b")]
    public ushort Blue { get; set; }

    public override string ToString() => $"#{Red:x2}{Green:x2}{Blue:x2}";
    public bool Equals(RgbColor? other) => other.HasValue && other.Value.Red == Red && other.Value.Green == Green && other.Value.Blue == Blue;
}
