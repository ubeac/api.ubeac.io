using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace uBeac.Repositories.MongoDB
{
    public class MongoDictionarySerializer : DictionarySerializerBase<Dictionary<string, object>, string, object>
    {
        public override Dictionary<string, object> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(context.Reader);
            var result = BsonExtensionMethods.ToJson(bsonDocument);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dictionary<string, object> value)
        {
            var jsonDocument = JsonConvert.SerializeObject(value);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

            BsonSerializer.Serialize(context.Writer, bsonDocument);
        }
    }

}
