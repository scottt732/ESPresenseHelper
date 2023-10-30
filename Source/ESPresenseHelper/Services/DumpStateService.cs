using ESPresenseHelper.State;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ESPresenseHelper.Services;

public class DumpStateService : BackgroundService
{
    private ESPresenseState State { get; }
    private ILogger<DumpStateService> Logger { get; }

    private JsonSerializer JsonSerializer { get; }

    public DumpStateService(
        ESPresenseState state,
        ILogger<DumpStateService> logger)
    {
        State = state;
        Logger = logger;

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        jsonSerializerSettings.Converters.Add(new StringEnumConverter());

        JsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        /*
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

            var sb = new StringBuilder();
            await using (var sw = new StringWriter(sb))
            {
                JsonSerializer.Serialize(sw, State.GetRoomsById());
            }

            await File.WriteAllTextAsync(@"c:\\temp\\rooms.json", sb.ToString(), stoppingToken);

            // Logger.LogInformation("RoomsById: {Json}", sb);

            sb.Length = 0;
            await using (var sw = new StringWriter(sb))
            {
                JsonSerializer.Serialize(sw, State.GetDevicesById());
            }

            await File.WriteAllTextAsync(@"c:\\temp\\devices.json", sb.ToString(), stoppingToken);

            // Logger.LogInformation("DevicesById: {Json}", sb);

            sb.Length = 0;
            await using (var sw = new StringWriter(sb))
            {
                JsonSerializer.Serialize(sw, State.GetSettingsByDeviceId());
            }

            await File.WriteAllTextAsync(@"c:\\temp\\settings.json", sb.ToString(), stoppingToken);

            // Logger.LogInformation("SettingsById: {Json}", sb);

            Logger.LogInformation("Dumped state to disk");
        }
        */
        return Task.CompletedTask;
    }
}
