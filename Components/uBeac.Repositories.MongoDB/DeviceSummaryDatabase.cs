using MongoDB.Driver;

namespace uBeac.Repositories.MongoDB
{
    public class DeviceSummaryDatabase : MongoDatabaseFactory
    {
        public DeviceSummaryDatabase(IMongoDatabase mongoDatabase): base(mongoDatabase)
        {
        }        
    }
}
