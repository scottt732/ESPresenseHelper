using ESPresenseHelper.Models;
using ESPresenseHelper.Serialization;
using ESPresenseHelper.State;
using Microsoft.Extensions.Logging;
using Sholo.Mqtt.Controllers;
using Sholo.Mqtt.ModelBinding.TypeConverters.Attributes;

namespace ESPresenseHelper.Controllers;

public class RoomsController : MqttControllerBase
{
    private ESPresenseState State { get; }
    private ILogger Logger { get; }

    public RoomsController(
        ESPresenseState state,
        ILogger<DevicesController> logger)
    {
        State = state;
        Logger = logger;
    }

    [Topic("espresense/rooms/+id/status")]
    public Task<bool> RoomStatusAsync(string id, string status)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.Status = status;
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/max_distance")]
    public Task<bool> MaxDistanceAsync(string id, float maxDistance)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.MaxDistance = maxDistance;
        Logger.LogDebug("Room {Id} max distance: {MaxDistance}", id, maxDistance);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/absorption")]
    public Task<bool> AbsorptionAsync(string id, float absorption)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.Absorption = absorption;
        Logger.LogDebug("Room {Id} absorption: {Absorption}", id, absorption);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/tx_ref_rssi")]
    public Task<bool> TxRefSssiAsync(string id, float txRefRssi)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.TxRefRssi = txRefRssi;
        Logger.LogDebug("Room {Id} tx ref rssi: {TxRefRssi}", id, txRefRssi);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/rx_adj_rssi")]
    public Task<bool> RxAdjRssiAsync(string id, float rxAdjRssi)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.RxAdjRssi = rxAdjRssi;
        Logger.LogDebug("Room {Id} rx adjusted rssi: {RxAdjRssi}", id, rxAdjRssi);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/arduino_ota")]
    public Task<bool> ArduinoOtaAsync(string id, [FromMqttPayload<OnOffBooleanParameterTypeConverter>(serviceKey: null)] bool arduinoOtaAdj)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.ArduinoOta = arduinoOtaAdj;
        Logger.LogDebug("Room {Id} arduino ota: {ArduinoOta}", id, arduinoOtaAdj);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/auto_update")]
    public Task<bool> AutoUpdateAsync(string id, [FromMqttPayload<OnOffBooleanParameterTypeConverter>(serviceKey: null)] bool autoUpdate)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.AutoUpdate = autoUpdate;
        Logger.LogDebug("Room {Id} auto update: {AutoUpdate}", id, autoUpdate);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/prerelease")]
    public Task<bool> PrereleaseAsync(string id, [FromMqttPayload<OnOffBooleanParameterTypeConverter>(serviceKey: null)] bool prerelease)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.Prerelease = prerelease;
        Logger.LogDebug("Room {Id} pre-release: {Prerelease}", id, prerelease);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/pir_timeout")]
    public Task<bool> PirTimeoutAsync(string id, float pirTimeout)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.PirTimeout = pirTimeout;
        Logger.LogDebug("Room {Id} PIR timeout: {PirTimeout}", id, pirTimeout);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/radar_timeout")]
    public Task<bool> RadarTimeoutAsync(string id, float radarTimeout)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.RadarTimeout = radarTimeout;
        Logger.LogDebug("Room {Id} Radar timeout: {RadarTimeout}", id, radarTimeout);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/led_1")]
    public Task<bool> Led1Async(string id, LedModel led)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.Led1 = led;
        Logger.LogDebug("Room {Id} LED: {State}, {Color}, {Brightness}", id, led.State, led.Color, led.Brightness);
        return Task.FromResult(true);
    }

    [Topic("espresense/rooms/+id/motion")]
    public Task<bool> MotionAsync(string id, [FromMqttPayload<OnOffBooleanParameterTypeConverter>(serviceKey: null)] bool motion)
    {
        var roomState = State.GetOrCreateRoomById(id);
        roomState.Motion = motion;
        Logger.LogDebug("Room {Id} motion: {Motion}", id, motion);
        return Task.FromResult(true);
    }
}
