using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.SensorProcessor.Repositories
{
    public interface IRepository
    {
        // todo: change this repository to accept IEnumerable<SensorData>, not gatewayData
        Task InsertSensorData(GatewayData gatewayData);
    }
}

namespace uBeac.SensorProcessor.Repositories.MongoDB
{
    public class Repository : IRepository
    {
        private readonly IMongoDatabase _database;
        private readonly string _sensorDataCollectionName;
        public Repository(SensorDataDatabase sensorDataDatabase)
        {
            _database = sensorDataDatabase.GetMongoDB();

            _sensorDataCollectionName = typeof(SensorData).Name;

            // After revise with Amir we decided to make this repository async and return Task, put await on insert and change write concern to Unacknowledged
            _database.WithWriteConcern(WriteConcern.Unacknowledged);
        }

        public async Task InsertSensorData(GatewayData gatewayData)
        {
            var sensorsList = gatewayData.Devices.SelectMany(y => y.Sensors);
            if (sensorsList.Count() > 0)
            {
                var sensorDataCollection = _database.GetCollection<SensorData>(_sensorDataCollectionName + "_" + gatewayData.TeamId.ToString().Replace('-', '_'));
                await sensorDataCollection.InsertManyAsync(sensorsList, new InsertManyOptions { IsOrdered = false });
            }
        }
    }
}
