using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ESPresenseHelper.Options;

[PublicAPI]
public class MonitorOptions
{
    [Required]
    public string MqttTopicRoot { get; set; } = "espresense";

    [Required]
    public string HelperMqttTopicRoot { get; set; } = "espresensehelper";

    public Collection<SettingsEntry> Nodes { get; } = new();

    public Collection<SettingsEntry> EnrolledDevices { get; } = new();
}
