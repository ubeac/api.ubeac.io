using MongoDB.Driver;

namespace uBeac.Repositories.MongoDB
{
    public class GatewayDataDatabase : MongoDatabaseFactory
    {
        public GatewayDataDatabase(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }
    }
}
