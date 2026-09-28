using MongoDB.Driver;

namespace uBeac.Repositories.MongoDB
{
    public class MainDatabase: MongoDatabaseFactory
    {
        public MainDatabase(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }
    }
}
