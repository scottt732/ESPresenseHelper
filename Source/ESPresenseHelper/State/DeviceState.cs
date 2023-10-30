using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using Sholo.Utils.Collections;

namespace ESPresenseHelper.State;

/// <summary>
/// Uses nuget package for INotifyPropertyChanged
/// </summary>
[PublicAPI]
public partial class DeviceState : INotifyPropertyChanged
{
    public string Id { get; }

    [Notify]
    private string? _status;

    [Notify]
    private RoomState? _room;

    [Notify]
    private SettingsState? _settings;

    [Notify]
    private DeviceReadingState? _closestNode;

    [Notify]
    private DeviceReadingState[] _closestNodes = null!;

    private LazyConcurrentDictionaryThing<string, DeviceReadingState> ReadingsByRoomId { get; } = new();

    public DeviceState(string id)
    {
        Id = id;
    }

    public DeviceReadingState GetOrCreateDeviceReadingState(RoomState roomState)
    {
        return ReadingsByRoomId.GetOrAdd(roomState.Id, () => new DeviceReadingState(roomState));
    }

    public string? RecalculateLocation()
    {
        ClosestNodes = ReadingsByRoomId
            .ToArray() // Snapshot ConcurrentDictionary
            .Where(x => !Id.EndsWith(x.Value.Mac, StringComparison.Ordinal))
            .OrderBy(x => x.Value.Distance)
            .Select(x => x.Value)
            .ToArray();

        if (ClosestNodes.Any())
        {
            var bestReading = ClosestNodes
                .FirstOrDefault();

            if (bestReading != default)
            {
                if (ClosestNode == null || bestReading.Mac != ClosestNode?.Mac)
                {
                    ClosestNode = bestReading;
                    return bestReading.Room.Id;
                }
            }
        }

        return null;
    }
}
