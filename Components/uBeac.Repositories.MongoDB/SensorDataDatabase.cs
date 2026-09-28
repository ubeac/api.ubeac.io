using MongoDB.Driver;

namespace uBeac.Repositories.MongoDB
{
    public class SensorDataDatabase : MongoDatabaseFactory
    {
        public SensorDataDatabase(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }
    }
}
