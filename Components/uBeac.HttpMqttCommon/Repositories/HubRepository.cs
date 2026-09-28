using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.HttpMqttCommon.Repositories
{
    public interface IHubRepository
    {
        Task<IEnumerable<Gateway>> GetAllGateways(CancellationToken cancellationToken = default);
        Task<IEnumerable<Team>> GetAllTeams(CancellationToken cancellationToken = default);
    }
}

namespace uBeac.HttpMqttCommon.Repositories.MongoDB
{
    public class HubRepository : IHubRepository
    {
        private readonly MainDatabase _database;
        private readonly IMongoCollection<Gateway> _gatewayCollection;
        private readonly IMongoCollection<Team> _teamCollection;

        public HubRepository(MainDatabase database)
        {
            _database = database;
            _gatewayCollection = _database.GetMongoDB().GetCollection<Gateway>(typeof(Gateway).Name);
            _teamCollection = _database.GetMongoDB().GetCollection<Team>(typeof(Team).Name);
        }
        
        public async Task<IEnumerable<Gateway>> GetAllGateways(CancellationToken cancellationToken = default)
        {
            return  (await _gatewayCollection.FindAsync(Builders<Gateway>.Filter.Empty, cancellationToken: cancellationToken)).ToEnumerable();
        }

        public async Task<IEnumerable<Team>> GetAllTeams(CancellationToken cancellationToken = default)
        {
            return (await _teamCollection.FindAsync(Builders<Team>.Filter.Empty, cancellationToken: cancellationToken)).ToEnumerable();
        }
    }
}
