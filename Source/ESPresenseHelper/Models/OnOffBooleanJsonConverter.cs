using Newtonsoft.Json;

namespace ESPresenseHelper.Models;

public class OnOffBooleanJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(bool) || objectType == typeof(bool?);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var val = reader.Value?.ToString()?.Trim();

        if (val == null)
        {
            if (objectType == typeof(bool))
            {
                return false;
            }

            return null!;
        }

        if (val.Equals(true.ToString(), StringComparison.OrdinalIgnoreCase) ||
            val.Equals("ON", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (val.Equals(false.ToString(), StringComparison.OrdinalIgnoreCase) ||
            val.Equals("OFF", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (objectType == typeof(bool))
        {
            return false;
        }

        return null!;
    }
}
