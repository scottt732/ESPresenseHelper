using System.Reactive.Concurrency;
using ESPresenseHelper.HealthChecks;
using ESPresenseHelper.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTnet.Server;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Sholo.CommandLine.Containers;
using Sholo.HomeAssistant.Client;
using Sholo.HomeAssistant.Client.Settings;
using Sholo.HomeAssistant.DependencyInjection;
using Sholo.HomeAssistant.Mqtt;
using Sholo.HomeAssistant.Mqtt.Discovery;
using Sholo.HomeAssistant.Mqtt.Discovery.Payloads;
using Sholo.Mqtt;
using Sholo.Mqtt.Settings;
using Sholo.Utils;
using Sholo.Utils.Validation;

namespace ESPresenseHelper;

public static class Program
{
    public static async Task Main(string[] args)
        => await ContainerizedHostBuilder.Create<ESPresenseHelperAppOptions>(args)
            .ConfigureWebHost(builder =>
            {
                builder.UseKestrel();
                builder.UseUrls("http://*:21210");
                builder.UseStaticWebAssets();

                builder.ConfigureServices((ctx, services) =>
                {
                    services.AddCors();
                    services.AddRazorPages();
                    services.AddServerSideBlazor();
                });

                builder.Configure((ctx, app) =>
                {
                    if (!ctx.HostingEnvironment.IsDevelopment())
                    {
                        app.UseExceptionHandler("/Error");
                    }

                    app.UseStaticFiles();
                    app.UseRouting();

                    // TODO: Better CORS policy
                    app.UseCors(b =>
                        b
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin());

                    app.UseOpenTelemetryPrometheusScrapingEndpoint();

                    app.UseHealthChecks("/healthz/ready", new HealthCheckOptions
                    {
                        Predicate = healthCheck => healthCheck.Tags.Contains("ready")
                    });

                    app.UseHealthChecks("/healthz/live", new HealthCheckOptions
                    {
                        Predicate = _ => false
                    });

                    app.UseEndpoints(erb =>
                    {
                        erb.MapBlazorHub();
                        erb.MapFallbackToPage("/_Host");
                    });
                });
            })
            .ConfigureServices((ctx, services) =>
            {
                services.AddOptions<DeviceOptions>()
                    .Bind(ctx.Configuration.GetSection("homeassistant:device"))
                    .Configure(opt =>
                    {
                        var applicationName = ctx.Properties?["ApplicationName"]?.ToString() ?? string.Empty;
                        var applicationVersion = ctx.Properties?["ApplicationVersion"]?.ToString() ?? string.Empty;

                        if (string.IsNullOrEmpty(opt.Identifier))
                        {
                            opt.Identifier = "ESPresenseHelper";
                        }

                        if (string.IsNullOrEmpty(opt.Manufacturer))
                        {
                            opt.Manufacturer = "https://github.com/scottt732";
                        }

                        if (string.IsNullOrEmpty(opt.Name))
                        {
                            opt.Name = applicationName;
                        }

                        if (string.IsNullOrEmpty(opt.SwVersion))
                        {
                            opt.SwVersion = applicationVersion;
                        }
                    })
                    .ValidateDataAnnotations(true)
                    .ValidateOnStart();

                services.AddSingleton(sp =>
                {
                    var deviceOptions = sp.GetRequiredService<IOptions<DeviceOptions>>();

                    var deviceBuilder = new DeviceBuilder()
                        .WithIdentifier(deviceOptions.Value.Identifier)
                        .WithManufacturer(deviceOptions.Value.Manufacturer)
                        .WithModel(deviceOptions.Value.Model)
                        .WithName(deviceOptions.Value.Name)
                        .WithSwVersion(deviceOptions.Value.SwVersion);

                    if (deviceOptions.Value.Connection is { ConnectionType: not null, ConnectionIdentifier: not null })
                    {
                        deviceBuilder.WithConnection(deviceOptions.Value.Connection.ConnectionType, deviceOptions.Value.Connection.ConnectionIdentifier);
                    }

                    var device = deviceBuilder.Build();
                    return device;
                });

                services.AddMqttConsumerService("mqtt");

                services.AddHomeAssistant(ctx.Configuration.GetSection("homeassistant"))
                    .AddMqtt()
                    .AddClient();

                services.AddOptions<MonitorOptions>()
                    .Bind(ctx.Configuration.GetSection("monitor"))
                    .ValidateDataAnnotations(true)
                    .ValidateOnStart();

                services.AddSingleton<IScheduler>(Scheduler.Default);

                /*
                services.AddHostedService<HomeAssistantStateChangeMonitorService>();
                services.AddHostedService<HeartbeatService>();
                services.AddHostedService<MqttMonitorService>();
                */

                services.PostConfigure<MonitorOptions>(c =>
                {
                    var mqttTopicRoot = Environment.GetEnvironmentVariable("MQTT_TOPIC_ROOT");
                    var activityLevelsRefreshIntervalStr = Environment.GetEnvironmentVariable("ACTIVITYLEVELS_REFRESH_INTERVAL");
                    var activityLevelsRefreshInterval = !string.IsNullOrEmpty(activityLevelsRefreshIntervalStr) && TimeSpan.TryParse(activityLevelsRefreshIntervalStr, out var refreshInterval) ? refreshInterval : null as TimeSpan?;

                    if (!string.IsNullOrEmpty(mqttTopicRoot)) { c.MqttTopicRoot = mqttTopicRoot; }

                    c.MqttTopicRoot ??= "espresensehelper";
                });

                services.PostConfigure<HomeAssistantClientOptions>(c =>
                {
                    var homeAssistantWebsocketsUrlStr = Environment.GetEnvironmentVariable("HA_CLIENT_WS_URL");
                    var homeAssistantWebsocketsUrl = !string.IsNullOrEmpty(homeAssistantWebsocketsUrlStr) && Uri.TryCreate(homeAssistantWebsocketsUrlStr, UriKind.Absolute, out var wsUrl) ? wsUrl : null;
                    var homeAssistantRestUrlPrefixStr = Environment.GetEnvironmentVariable("HA_CLIENT_REST_URL_PREFIX");
                    var homeAssistantRestUrlPrefix = !string.IsNullOrEmpty(homeAssistantRestUrlPrefixStr) && Uri.TryCreate(homeAssistantRestUrlPrefixStr, UriKind.Absolute, out var restUrl) ? restUrl : null;
                    var homeAssistantAuthToken = Environment.GetEnvironmentVariable("HA_CLIENT_AUTH_TOKEN");
                    var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");

                    if (homeAssistantWebsocketsUrl != null) { c.WsUrl = homeAssistantWebsocketsUrl; }
                    if (homeAssistantRestUrlPrefix != null) { c.RestApiUrlPrefix = homeAssistantRestUrlPrefix; }
                    if (!string.IsNullOrEmpty(homeAssistantAuthToken)) { c.Auth.Token = homeAssistantAuthToken; }
                    if (!string.IsNullOrEmpty(supervisorToken)) { c.Auth.Token = supervisorToken; }

                    c.WsUrl ??= new Uri("ws://supervisor/core/websocket");
                    c.RestApiUrlPrefix ??= new Uri("http://supervisor/core/api");
                });

                services.PostConfigure<HomeAssistantDiscoveryClientSettings>(c =>
                {
                    var homeAssistantMqttDiscoveryPrefix = Environment.GetEnvironmentVariable("HA_MQTT_DISCOVERY_PREFIX");
                    var homeAssistantMqttDiscoveryQosStr = Environment.GetEnvironmentVariable("HA_MQTT_DISCOVERY_QOS");
                    var homeAssistantMqttDiscoveryQos = !string.IsNullOrEmpty(homeAssistantMqttDiscoveryQosStr) && Enum.TryParse<MqttQualityOfServiceLevel>(homeAssistantMqttDiscoveryQosStr, out var discoQos) ? discoQos : null as MqttQualityOfServiceLevel?;
                    var homeAssistantMqttDiscoveryRetainStr = Environment.GetEnvironmentVariable("HA_MQTT_DISCOVERY_RETAIN");
                    var homeAssistantMqttDiscoveryRetain = !string.IsNullOrEmpty(homeAssistantMqttDiscoveryRetainStr) && bool.TryParse(homeAssistantMqttDiscoveryRetainStr, out var onlineRetain) ? onlineRetain : null as bool?;

                    if (!string.IsNullOrEmpty(homeAssistantMqttDiscoveryPrefix)) { c.DiscoveryPrefix = homeAssistantMqttDiscoveryPrefix; }
                    if (homeAssistantMqttDiscoveryQos != null) { c.QualityOfService = homeAssistantMqttDiscoveryQos; }
                    if (homeAssistantMqttDiscoveryRetain != null) { c.Retain = homeAssistantMqttDiscoveryRetain.Value; }

                    c.DiscoveryPrefix ??= "homeassistant";
                    c.QualityOfService ??= MqttQualityOfServiceLevel.AtMostOnce;
                    c.Retain ??= false;
                });

                services.PostConfigure<ManagedMqttSettings>(c =>
                {
                    var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST");
                    var mqttClientId = Environment.GetEnvironmentVariable("MQTT_CLIENT_ID");
                    var mqttPortStr = Environment.GetEnvironmentVariable("MQTT_PORT");
                    var mqttPort = !string.IsNullOrEmpty(mqttPortStr) && int.TryParse(mqttPortStr, out var port) ? port : null as int?;
                    var mqttUseTlsStr = Environment.GetEnvironmentVariable("MQTT_USE_TLS");
                    var mqttUseTls = !string.IsNullOrEmpty(mqttUseTlsStr) && bool.TryParse(mqttUseTlsStr, out var useTls) ? useTls : null as bool?;
                    var mqttClientCertificatePrivateKey = Environment.GetEnvironmentVariable("MQTT_CLIENT_CERT_PRIVATE_KEY");
                    var mqttClientCertificatePublicKey = Environment.GetEnvironmentVariable("MQTT_CLIENT_CERT_PUBLIC_KEY");
                    var mqttIgnoreCertificateValidationErorrsStr = Environment.GetEnvironmentVariable("MQTT_IGNORE_CERT_VALIDATION_ERRORS");
                    var mqttIgnoreCertificateValidationErorrs = !string.IsNullOrEmpty(mqttIgnoreCertificateValidationErorrsStr) && bool.TryParse(mqttIgnoreCertificateValidationErorrsStr, out var ignore) ? ignore : null as bool?;
                    var mqttUsername = Environment.GetEnvironmentVariable("MQTT_USERNAME");
                    var mqttPassword = Environment.GetEnvironmentVariable("MQTT_PASSWORD");
                    var mqttOnlineMessageTopic = Environment.GetEnvironmentVariable("MQTT_ONLINE_MSG_TOPIC");
                    var mqttOnlineMessagePayload = Environment.GetEnvironmentVariable("MQTT_ONLINE_MSG_PAYLOAD");
                    var mqttOnlineMessageQosStr = Environment.GetEnvironmentVariable("MQTT_ONLINE_MSG_QOS");
                    var mqttOnlineMessageQos = !string.IsNullOrEmpty(mqttOnlineMessageQosStr) && Enum.TryParse<MqttQualityOfServiceLevel>(mqttOnlineMessageQosStr, out var onlineQos) ? onlineQos : null as MqttQualityOfServiceLevel?;
                    var mqttOnlineMessageRetainStr = Environment.GetEnvironmentVariable("MQTT_ONLINE_MSG_RETAIN");
                    var mqttOnlineMessageRetain = !string.IsNullOrEmpty(mqttOnlineMessageRetainStr) && bool.TryParse(mqttOnlineMessageRetainStr, out var onlineRetain) ? onlineRetain : null as bool?;
                    var mqttLwtMessageTopic = Environment.GetEnvironmentVariable("MQTT_LWT_MSG_TOPIC");
                    var mqttLwtMessagePayload = Environment.GetEnvironmentVariable("MQTT_LWT_MSG_PAYLOAD");
                    var mqttLwtMessageQosStr = Environment.GetEnvironmentVariable("MQTT_LWT_MSG_QOS");
                    var mqttLwtMessageQos = !string.IsNullOrEmpty(mqttLwtMessageQosStr) && Enum.TryParse<MqttQualityOfServiceLevel>(mqttLwtMessageQosStr, out var lwtQos) ? lwtQos : null as MqttQualityOfServiceLevel?;
                    var mqttLwtMessageRetainStr = Environment.GetEnvironmentVariable("MQTT_LWT_MSG_RETAIN");
                    var mqttLwtMessageRetain = !string.IsNullOrEmpty(mqttLwtMessageRetainStr) && bool.TryParse(mqttLwtMessageRetainStr, out var lwtRetain) ? lwtRetain : null as bool?;
                    var mqttAutoReconnectDelayStr = Environment.GetEnvironmentVariable("MQTT_AUTO_RECONNECT_DELAY");
                    var mqttAutoReconnectDelay = !string.IsNullOrEmpty(mqttAutoReconnectDelayStr) && TimeSpan.TryParse(mqttAutoReconnectDelayStr, out var autoReconnectDelay) ? autoReconnectDelay : null as TimeSpan?;
                    var mqttKeepAliveIntervalStr = Environment.GetEnvironmentVariable("MQTT_KEEP_ALIVE_INTERVAL");
                    var mqttKeepAliveInterval = !string.IsNullOrEmpty(mqttKeepAliveIntervalStr) && TimeSpan.TryParse(mqttKeepAliveIntervalStr, out var keepAliveInterval) ? keepAliveInterval : null as TimeSpan?;
                    var mqttTimeoutStr = Environment.GetEnvironmentVariable("MQTT_TIMEOUT");
                    var mqttTimeout = !string.IsNullOrEmpty(mqttTimeoutStr) && TimeSpan.TryParse(mqttTimeoutStr, out var timeout) ? timeout : null as TimeSpan?;
                    var mqttMaxPendingMessagesStr = Environment.GetEnvironmentVariable("MQTT_MAX_PENDING_MESSAGES");
                    var mqttMaxPendingMessages = !string.IsNullOrEmpty(mqttMaxPendingMessagesStr) && int.TryParse(mqttMaxPendingMessagesStr, out var maxPendingMessages) ? maxPendingMessages : null as int?;
                    var mqttPendingMessagesOverflowStrategyStr = Environment.GetEnvironmentVariable("MQTT_PENDING_MESSAGES_OVERFLOW_STRATEGY");
                    var mqttPendingMessagesOverflowStrategy = !string.IsNullOrEmpty(mqttPendingMessagesOverflowStrategyStr) && Enum.TryParse<MqttPendingMessagesOverflowStrategy>(mqttPendingMessagesOverflowStrategyStr, out var pendingMessagesOverflowStrategy) ? pendingMessagesOverflowStrategy : null as MqttPendingMessagesOverflowStrategy?;
                    var mqttProtocolVersionStr = Environment.GetEnvironmentVariable("MQTT_PROTOCOL_VERSION");
                    var mqttProtocolVersion = !string.IsNullOrEmpty(mqttProtocolVersionStr) && Enum.TryParse<MqttProtocolVersion>(mqttProtocolVersionStr, out var protocolVersion) ? protocolVersion : null as MqttProtocolVersion?;

                    if (!string.IsNullOrEmpty(mqttClientId)) { c.ClientId = mqttClientId; }
                    if (!string.IsNullOrEmpty(mqttHost)) { c.Host = mqttHost; }
                    if (mqttPort != null) { c.Port = mqttPort.Value; }
                    if (mqttUseTls != null) { c.UseTls = mqttUseTls.Value; }

                    if (mqttClientCertificatePrivateKey != null && mqttClientCertificatePublicKey != null)
                    {
                        c.ClientCertificatePrivateKey = mqttClientCertificatePrivateKey;
                        c.ClientCertificatePublicKey = mqttClientCertificatePublicKey;
                    }

                    if (mqttIgnoreCertificateValidationErorrs != null) { c.IgnoreCertificateValidationErorrs = mqttIgnoreCertificateValidationErorrs.Value; }
                    if (!string.IsNullOrEmpty(mqttUsername)) { c.Username = mqttUsername; }
                    if (!string.IsNullOrEmpty(mqttPassword)) { c.Password = mqttPassword; }
                    if (!string.IsNullOrEmpty(mqttOnlineMessageTopic)) { c.OnlineMessage.Topic = mqttOnlineMessageTopic; }
                    if (!string.IsNullOrEmpty(mqttOnlineMessagePayload)) { c.OnlineMessage.Payload = mqttOnlineMessagePayload; }
                    if (mqttOnlineMessageQos != null) { c.OnlineMessage.QualityOfServiceLevel = mqttOnlineMessageQos; }
                    if (mqttOnlineMessageRetain != null) { c.OnlineMessage.Retain = mqttOnlineMessageRetain; }
                    if (!string.IsNullOrEmpty(mqttLwtMessageTopic)) { c.LastWillAndTestament.Topic = mqttLwtMessageTopic; }
                    if (!string.IsNullOrEmpty(mqttLwtMessagePayload)) { c.LastWillAndTestament.Payload = mqttLwtMessagePayload; }
                    if (mqttLwtMessageQos != null) { c.LastWillAndTestament.QualityOfServiceLevel = mqttLwtMessageQos; }
                    if (mqttLwtMessageRetain != null) { c.LastWillAndTestament.Retain = mqttLwtMessageRetain; }
                    if (mqttAutoReconnectDelay != null) { c.AutoReconnectDelay = mqttAutoReconnectDelay.Value; }
                    if (mqttKeepAliveInterval != null) { c.KeepAliveInterval = mqttKeepAliveInterval.Value; }
                    if (mqttTimeout != null) { c.Timeout = mqttTimeout.Value; }
                    if (mqttMaxPendingMessages != null) { c.MaxPendingMessages = mqttMaxPendingMessages.Value; }
                    if (mqttPendingMessagesOverflowStrategy != null) { c.PendingMessagesOverflowStrategy = mqttPendingMessagesOverflowStrategy.Value; }
                    if (mqttProtocolVersion != null) { c.MqttProtocolVersion = mqttProtocolVersion.Value; }

                    c.OnlineMessage.Topic ??= "activitylevels/availability";
                    c.OnlineMessage.Payload ??= "online";
                    c.OnlineMessage.QualityOfServiceLevel ??= MqttQualityOfServiceLevel.AtLeastOnce;
                    c.OnlineMessage.Retain ??= true;

                    c.LastWillAndTestament.Topic ??= "activitylevels/availability";
                    c.LastWillAndTestament.Payload ??= "offline";
                    c.LastWillAndTestament.QualityOfServiceLevel ??= MqttQualityOfServiceLevel.AtLeastOnce;
                    c.LastWillAndTestament.Retain ??= true;
                });

                var applicationName = ctx.Properties?["ApplicationName"]?.ToString() ?? string.Empty;
                var applicationVersion = ctx.Properties?["ApplicationVersion"]?.ToString() ?? string.Empty;

                services.AddSingleton<ReadinessCheck>();

                services.AddHealthChecks()
                    .AddCheck<ReadinessCheck>("Startup", tags: new[] { "ready" });

                services.AddSingleton<IMeters>(sp => new Meters(applicationName, applicationVersion));

                services.AddOpenTelemetry()
                    .ConfigureResource(resource => resource.AddService(serviceName: "ESPresenseHelper"))
                    .WithMetrics(metrics => metrics
                        .AddMeter("ESPresenseHelper")
                        .AddAspNetCoreInstrumentation()
                        .AddPrometheusExporter());
            })
            .Build()
            .RunAsync();
}
