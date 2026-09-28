using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Collections.Generic;

namespace uBeac.Repositories.MongoDB
{

    public class MongoGeoJsonSerializer : DictionarySerializerBase<Dictionary<string, decimal>, string, decimal>
    {
        public override Dictionary<string, decimal> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(context.Reader);

            try
            {
                if (bsonDocument.Contains("coordinates") && bsonDocument.Contains("type") && bsonDocument["type"].BsonType == BsonType.String)
                {
                    var geoBson = BsonSerializer.Deserialize<GeoJsonPoint<GeoJson2DGeographicCoordinates>>(bsonDocument);
                    return new Dictionary<string, decimal> { { "lat", (decimal)geoBson.Coordinates.Latitude }, { "long", (decimal)geoBson.Coordinates.Longitude } };
                }
            }
            catch (System.Exception)
            {
            }

            return BsonSerializer.Deserialize<Dictionary<string, decimal>>(bsonDocument);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dictionary<string, decimal> value)
        {
            if (value != null && value.Count == 2 && value.ContainsKey("lat") && value.ContainsKey("long"))
            {
                var geo = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates((double)value["long"], (double)value["lat"]));
                BsonSerializer.Serialize(context.Writer, geo);
                return;
            }

            base.Serialize(context, args, value);
        }
    }
}
