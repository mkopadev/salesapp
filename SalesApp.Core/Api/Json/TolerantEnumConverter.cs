using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SalesApp.Core.Api.Json
{
    public class TolerantEnumConverter : JsonConverter
    {
        /*public override bool CanConvert(Type objectType)
        {/*
            Type type = IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
            return type.IsEnum;#1#
        }*/

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            /*bool isNullable = this.IsNullableType(objectType);
            Type enumType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;
*/
            Type enumType = objectType;
            string[] names = Enum.GetNames(enumType);

            if (reader.TokenType == JsonToken.String)
            {
                string enumText = reader.Value.ToString();

                if (!string.IsNullOrEmpty(enumText))
                {
                    string match = names
                        .Where(n => string.Equals(n, enumText, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (match != null)
                    {
                        return Enum.Parse(enumType, match);
                    }
                }
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                int enumVal = Convert.ToInt32(reader.Value);
                int[] values = (int[])Enum.GetValues(enumType);
                if (values.Contains(enumVal))
                {
                    return Enum.Parse(enumType, enumVal.ToString());
                }
            }
            
            string defaultName = names
                .Where(n => string.Equals(n, "Unknown", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (defaultName == null)
            {
                defaultName = names.First();
            }

            return Enum.Parse(enumType, defaultName);
          
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsEnum;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        /*private bool IsNullableType(Type t)
        {
            return (t.Is && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }*/
    }
}
