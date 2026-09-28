using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface ITeamRepository : IBaseEntityRepository<Team>
    {
        Task<Team> GetByNamespaceAsync(string teamNamespace, CancellationToken cancellationToken);
        Task<bool> UpdateTokensAsync(Guid teamId, Token token, CancellationToken cancellationToken);
        Task<int> HasAccessAsync(Guid teamId, string token, CancellationToken cancellationToken);
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class TeamRepository : BaseEntityRepository<Team>, ITeamRepository
    {
        public TeamRepository(MainDatabase database) : base(database)
        {
        }

        public async Task<Team> GetByNamespaceAsync(string teamNamespace, CancellationToken cancellationToken)
        {
            var filter = Builders<Team>.Filter;
            var query = filter.Eq(x => x.Namespace, teamNamespace);
            var result = await Collection.FindAsync(query, cancellationToken: cancellationToken);
            return result.FirstOrDefault();
        }

        public async Task<bool> UpdateTokensAsync(Guid teamId, Token token, CancellationToken cancellationToken)
        {
            var update = Builders<Team>.Update.AddToSet(x => x.Tokens, token);
            var filter = Builders<Team>.Filter.Eq(x => x.Id, teamId);

            var result = await Collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }, cancellationToken: cancellationToken);

            return result.IsAcknowledged;
        }

        public async Task<int> HasAccessAsync(Guid teamId, string token, CancellationToken cancellationToken)
        {
            var filter = Builders<Team>.Filter.Eq(x => x.Id, teamId);
            var userinfo = (await Collection.FindAsync(filter, cancellationToken: cancellationToken)).FirstOrDefault();
            if (userinfo != null)
            {
                return (int)userinfo.Tokens.Where(x => x.AccessToken == token).Select(x => x.Role).FirstOrDefault();
            }

            return -1;
        }
    }
}