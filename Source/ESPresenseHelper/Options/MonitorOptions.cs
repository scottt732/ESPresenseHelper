using System.ComponentModel.DataAnnotations;

namespace ESPresenseHelper.Options;

[PublicAPI]
public class MonitorOptions
{
    [Required]
    public string MqttTopicRoot { get; set; } = "espresensehelper";
}
