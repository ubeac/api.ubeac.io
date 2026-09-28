using MongoDB.Driver;

namespace uBeac.Repositories.MongoDB
{
    public interface IMongoFactory
    {
        IMongoDatabase GetMongoDB();
    }
}
