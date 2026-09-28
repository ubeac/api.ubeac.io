using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using uBeac.Repositories;
using uBeac.Repositories.MongoDB;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoConfigurationServicesExtensions
    {
        public static IServiceCollection AddMongo<TMongoDatabase>(this IServiceCollection services, string connectionStringName) where TMongoDatabase : class, IMongoFactory
        {
            services.AddSingleton<BsonSerializerRegistrar>();

            services.AddSingleton(provider =>
            {
                var bsonSerializerRegistrarx = provider.GetService<BsonSerializerRegistrar>();
                var configuration = provider.GetService<IConfiguration>();
                var mongoUrl = new MongoUrl(configuration.GetConnectionString(connectionStringName));
                var client = new MongoClient(mongoUrl);
                var mongoDB = client.GetDatabase(mongoUrl.DatabaseName);
                return ActivatorUtilities.CreateInstance<TMongoDatabase>(provider, mongoDB);
            });

            return services;

        }

        public static IServiceCollection AddMongoChangeTracker<TMongoDatabase, TKey, TValue>(this IServiceCollection services) where TMongoDatabase : class, IMongoFactory
        {
            services.AddSingleton<IChangeTracker<TKey, TValue>>(provider =>
           {
               var databaseFactory = provider.GetService<TMongoDatabase>();
               return ActivatorUtilities.CreateInstance<ChangeTracker<TKey, TValue>>(provider, databaseFactory);
           });

            return services;
        }
        
    }
}
