using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ESPresenseHelper.Models;

[PublicAPI]
public class SettingsModel
{
    [Required]
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [Required]
    [JsonProperty("name")]
    public string Name { get; set; } = null!;
}
