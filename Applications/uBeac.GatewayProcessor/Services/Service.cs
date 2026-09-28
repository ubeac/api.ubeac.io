using System;
using System.Threading.Tasks;
using uBeac.GatewayProcessor.Repositories;
using uBeac.Models;

namespace uBeac.GatewayProcessor.Services
{
    public interface IService
    {
        Task UpdateGateway(Guid gatewayId, DateTime lastRequestDate);
        Task InsertGatewayData(GatewayData gatewayData);
    }

    public class Service : IService
    {
        private readonly IRepository _repository;
        public Service(IRepository repository)
        {
            _repository = repository;
        }

        public async Task UpdateGateway(Guid gatewayId, DateTime lastRequestDate)
        {
            await _repository.UpdateGateway(gatewayId, lastRequestDate);
        }

        public async Task InsertGatewayData(GatewayData gatewayData)
        {
            await _repository.InsertGatewayData(gatewayData);
        }

    }
}
