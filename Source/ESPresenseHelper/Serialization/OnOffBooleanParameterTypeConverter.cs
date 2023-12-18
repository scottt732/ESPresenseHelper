using System.Text;
using Sholo.Mqtt.ModelBinding.TypeConverters;

namespace ESPresenseHelper.Serialization;

public class OnOffBooleanParameterTypeConverter : IMqttTopicArgumentTypeConverter, IMqttPayloadTypeConverter
{
    public bool TryConvertTopicArgument(string argument, Type targetType, out object result)
    {
        if (targetType != typeof(bool))
        {
            result = null!;
            return false;
        }

        if (targetType == typeof(bool?) && string.IsNullOrEmpty(argument))
        {
            result = null!;
            return true;
        }

        if (argument.Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (argument.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        result = null!;
        return false;
    }

    public bool TryConvertPayload(ArraySegment<byte> payload, Type targetType, out object result)
    {
        if (targetType != typeof(bool))
        {
            result = null!;
            return false;
        }

        if (targetType == typeof(bool?) && payload.Array == null)
        {
            result = null!;
            return true;
        }

        var value = Encoding.ASCII.GetString(payload.Array!, payload.Offset, payload.Count);

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
