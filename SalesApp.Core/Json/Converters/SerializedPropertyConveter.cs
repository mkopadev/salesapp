using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SalesApp.Core.Json.Converters
{
    public class SerializedPropertyConveter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JToken.Load(reader).ToString(Formatting.None);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((string)value);
        }
    }
}
