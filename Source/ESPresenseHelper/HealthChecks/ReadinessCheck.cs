using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ESPresenseHelper.HealthChecks;

public class ReadinessCheck : IHealthCheck
{
    private volatile bool _stateRestored;
    private volatile bool _websocketConnected;
    private volatile bool _mqttBrokerConnected;

    public bool StateRestored
    {
        get => _stateRestored;
        set => _stateRestored = value;
    }

    public bool WebsocketConnected
    {
        get => _websocketConnected;
        set => _websocketConnected = value;
    }

    public bool MqttBrokerConnected
    {
        get => _mqttBrokerConnected;
        set => _mqttBrokerConnected = value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stateRestored = StateRestored;
        var websocketConnected = WebsocketConnected;
        var mqttBrokerConnected = MqttBrokerConnected;

        if (websocketConnected && stateRestored && mqttBrokerConnected)
        {
            return Task.FromResult(HealthCheckResult.Healthy("The websocket has connected."));
        }

        var sb = new StringBuilder("Waiting for: ");

        if (!websocketConnected)
        {
            sb.Append("websocket connected, ");
        }

        if (!mqttBrokerConnected)
        {
            sb.Append("mqttBrokerConnected, ");
        }

        if (!stateRestored)
        {
            sb.Append("state restore, ");
        }

        sb.Length -= 2;

        return Task.FromResult(HealthCheckResult.Unhealthy(sb.ToString()));
    }
}
