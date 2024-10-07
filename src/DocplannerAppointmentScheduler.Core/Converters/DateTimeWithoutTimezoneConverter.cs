using Newtonsoft.Json;
using System;

public class DateTimeWithoutTimezoneConverter : JsonConverter<DateTime>
{
    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
        {
            throw new JsonSerializationException("DateTime value is null.");
        }
        return DateTime.Parse(reader.Value.ToString());
    }
}

