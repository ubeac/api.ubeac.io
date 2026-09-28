using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace uBeac.Serialization
{
    public class CustomDecimalDictionarySerializer : CustomCreationConverter<Dictionary<string, decimal>>
    {
        public override bool CanWrite => true;
        public override Dictionary<string, decimal> Create(Type objectType)
        {
            return new Dictionary<string, decimal>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var item in (Dictionary<string, decimal>)value)
            {
                writer.WritePropertyName(item.Key);
                writer.WriteValue(item.Value);
            }

            writer.WriteEndObject();
        }
    }
    public class CustomDictionarySerializer : CustomCreationConverter<Dictionary<string, object>>
    {
        //public override bool CanWrite => true;
        public override Dictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>();
        }

        public override bool CanConvert(Type objectType)
        {
            // in addition to handling IDictionary<string, object>
            // we want to handle the deserialization of dict value
            // which is of type object
            var x = objectType == typeof(object) || base.CanConvert(objectType);
            return x;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.Null)
                return base.ReadJson(reader, objectType, existingValue, serializer);

            // if the next token is not an object
            // then fall back on standard deserializer (strings, numbers etc.)
            var y = serializer.Deserialize(reader);
            return y;
        }

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    //if (CanConvert(value.GetType()))
        //    //{
        //    var value1 = (Dictionary<string, object>)value;

        //    //if (value1 != null && value1.Count > 0)
        //    //{
        //    writer.WriteStartObject();
        //    foreach (var item in value1)
        //    {
        //        if (item.Value is Dictionary<string, object> || item.Value is JObject)
        //        {
        //            writer.WritePropertyName(item.Key.ToString());
        //            WriteJson(writer, item.Value, serializer);
        //        }
        //        else
        //        {
        //            writer.WritePropertyName(item.Key.ToString());
        //            writer.WriteValue(item.Value);
        //        }
        //    }

        //    writer.WriteEndObject();
        //    //}
        //    //}
        //}
    }
}
