using MongoDB.Driver;

namespace uBeac.Repositories.MongoDB
{
    public class MongoDatabaseFactory : IMongoFactory
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDatabaseFactory(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public IMongoDatabase GetMongoDB() => _mongoDatabase;
    }
}
