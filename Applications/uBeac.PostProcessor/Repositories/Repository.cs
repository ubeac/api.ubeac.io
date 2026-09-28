using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.PostProcessor.Repositories
{
    public interface IRepository
    {
        Task<IEnumerable<Team>> GetTeamsAsync();
        Task<IEnumerable<Device>> GetDevicesAsync();
        Task<IEnumerable<Sensor>> GetSensorsAsync();
        Task UpdateSensorSchema(Guid sensorId, bool persist, List<string> schema);
        Task InsertManyDeviceAsync(IEnumerable<Device> devices);
        Task InsertManySensorAsync(IEnumerable<Sensor> sensors);
    }
}

namespace uBeac.PostProcessor.Repositories.MongoDB
{
    public class Repository : IRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Team> TeamCollection;
        private readonly IMongoCollection<Device> DeviceCollection;
        private readonly IMongoCollection<Sensor> SensorCollection;

        public Repository(MainDatabase database)
        {
            _database = database.GetMongoDB();

            TeamCollection = _database.GetCollection<Team>(typeof(Team).Name);
            DeviceCollection = _database.GetCollection<Device>(typeof(Device).Name);
            SensorCollection = _database.GetCollection<Sensor>(typeof(Sensor).Name);
        }

        public async Task<IEnumerable<Team>> GetTeamsAsync()
        {
            return (await TeamCollection.FindAsync(Builders<Team>.Filter.Empty)).ToEnumerable();
        }

        public async Task<IEnumerable<Device>> GetDevicesAsync()
        {
            return (await DeviceCollection.FindAsync(Builders<Device>.Filter.Empty)).ToEnumerable();
        }

        public async Task<IEnumerable<Sensor>> GetSensorsAsync()
        {
            return (await SensorCollection.FindAsync(Builders<Sensor>.Filter.Empty)).ToEnumerable();
        }

        public async Task UpdateSensorSchema(Guid sensorId, bool persist, List<string> schema)
        {
            var filter = Builders<Sensor>.Filter.Eq(x => x.Id, sensorId);
            var update = Builders<Sensor>.Update.Set(x => x.Schema, schema).Set(x=>x.Persist, persist);

            await SensorCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertManyDeviceAsync(IEnumerable<Device> devices)
        {
            DeviceCollection.WithWriteConcern(new WriteConcern(1, null, false, true));
            await DeviceCollection.InsertManyAsync(devices);
        }

        public async Task InsertManySensorAsync(IEnumerable<Sensor> sensors)
        {
            SensorCollection.WithWriteConcern(new WriteConcern(1, null, false, true));
            await SensorCollection.InsertManyAsync(sensors);
        }

    }
}
