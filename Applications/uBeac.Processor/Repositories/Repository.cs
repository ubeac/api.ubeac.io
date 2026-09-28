using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Processor.Repositories
{
    public interface IRepository
    {
        Task<List<Firmware>> GetAllFirmwares(CancellationToken cancellationToken = default);
    }
}

namespace uBeac.Processor.Repositories.MongoDB
{
    public class Repository : IRepository
    {

        private readonly IMongoDatabase _firmwareDataDatabase;
        private readonly IMongoCollection<Firmware> _firmwareCollection;

        public Repository(MainDatabase mainDatabase)
        {
            _firmwareDataDatabase = mainDatabase.GetMongoDB();
            _firmwareCollection = _firmwareDataDatabase.GetCollection<Firmware>(typeof(Firmware).Name);
        }

        public async Task<List<Firmware>> GetAllFirmwares(CancellationToken cancellationToken = default)
        {
            return await _firmwareCollection.AsQueryable().ToListAsync();
        }
    }

}
