using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.GatewayProcessor.Repositories
{
    public interface IRepository
    {
        Task UpdateGateway(Guid gatewayId, DateTime lastRequestDate);
        Task InsertGatewayData(GatewayData gatewayData);
    }
}

namespace uBeac.GatewayProcessor.Repositories.MongoDB
{
    public class Repository : IRepository
    {
        private readonly IMongoCollection<BsonDocument> _gatewayCollection;
        private readonly IMongoDatabase _gatewayDataDatabase;
        private readonly IMongoDatabase _gatewayDatabase;
        private readonly string _gatewayCollectionName;
        private readonly string _gatewayDataCollectionName;

        public Repository(MainDatabase mainDatabase, GatewayDataDatabase gatewayDataDatabase)
        {
            // preparing gateway collection in general uBeac database
            _gatewayDataDatabase = gatewayDataDatabase.GetMongoDB();
            _gatewayDatabase = mainDatabase.GetMongoDB();

            _gatewayDataCollectionName = typeof(GatewayData).Name;
            _gatewayCollectionName = typeof(Gateway).Name;

            _gatewayCollection = _gatewayDatabase.GetCollection<BsonDocument>(_gatewayCollectionName);

            // Based on speed analytics by Amir, we made decision to set write concern for this repository to Unacknowledged
            _gatewayDatabase.WithWriteConcern(WriteConcern.Unacknowledged);
            _gatewayDataDatabase.WithWriteConcern(WriteConcern.Unacknowledged);
        }

        public async Task InsertGatewayData(GatewayData gatewayData)
        {
            var collectionPostfix = gatewayData.TeamId.ToString().Replace('-', '_');
            var gatewayDataCollection = _gatewayDataDatabase.GetCollection<GatewayData>(_gatewayDataCollectionName + "_" + collectionPostfix);
            await gatewayDataCollection.InsertOneAsync(gatewayData);
        }

        public async Task UpdateGateway(Guid gatewayId, DateTime lastRequestDate)
        {
            var updateQuery = Builders<BsonDocument>.Update.Set("LastRequestDate", lastRequestDate).Inc("RequestCount", 1);
            var findQuery = Builders<BsonDocument>.Filter.Eq("_id", gatewayId);
            await _gatewayCollection.UpdateOneAsync(findQuery, updateQuery);            
        }
    }
}
