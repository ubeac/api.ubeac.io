using uBeac.SensorProcessor.Repositories;
using uBeac.Models;
using System.Threading.Tasks;

namespace uBeac.SensorProcessor.Services
{
    public interface IService
    {
        // todo: change this repository to accept IEnumerable<SensorData>, not gatewayData
        Task InsertSensorData(GatewayData gatewayData);
    }

    public class Service : IService
    {
        private readonly IRepository _repository;

        public Service(IRepository repository)
        {
            _repository = repository;
        }

        public async Task InsertSensorData(GatewayData gatewayData)
        {
            await _repository.InsertSensorData(gatewayData);
        }
    }
}
