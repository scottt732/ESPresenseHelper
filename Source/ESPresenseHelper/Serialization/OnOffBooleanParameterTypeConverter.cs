using System.Text;
using Sholo.Mqtt.TypeConverters;

namespace ESPresenseHelper.Serialization;

public class OnOffBooleanParameterTypeConverter : IMqttParameterTypeConverter, IMqttRequestPayloadTypeConverter
{
    public bool TryConvert(string value, Type targetType, out object result)
    {
        if (targetType != typeof(bool))
        {
            result = null!;
            return false;
        }

        if (targetType == typeof(bool?) && string.IsNullOrEmpty(value))
        {
            result = null!;
            return true;
        }

        if (value.Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (value.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        result = null!;
        return false;
    }

    public bool TryConvertPayload(ArraySegment<byte> payloadData, Type targetType, out object result)
    {
        if (targetType != typeof(bool))
        {
            result = null!;
            return false;
        }

        if (targetType == typeof(bool?) && payloadData.Array == null)
        {
            result = null!;
            return true;
        }

        var value = Encoding.ASCII.GetString(payloadData.Array!, payloadData.Offset, payloadData.Count);

        if (value.Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (value.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        result = null!;
        return false;
    }
}
