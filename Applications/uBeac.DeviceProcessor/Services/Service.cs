using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.DeviceProcessor.Repositories;
using uBeac.Models;

namespace uBeac.DeviceProcessor.Services
{
    public interface IService
    {
        Task UpdateDeviceSummary(GatewayData gatewayData);
    }

    public class Service : IService
    {
        private readonly IRepository _repository;

        public Service(IRepository repository)
        {
            _repository = repository;
        }

        public async Task UpdateDeviceSummary(GatewayData gatewayData)
        {
            var deviceSummary = new Dictionary<Guid, DeviceSummary>();
            foreach (var deviceData in gatewayData.Devices)
            {
                if (deviceSummary.ContainsKey(deviceData.Id))
                {
                    deviceSummary[deviceData.Id].RequestCount += 1;
                    deviceSummary[deviceData.Id].LastRequestDate = deviceData.DateTime;
                }
                else
                    deviceSummary.TryAdd(deviceData.Id, new DeviceSummary(deviceData, gatewayData.TeamId, gatewayData.FloorId));
            }

            if (deviceSummary.Count > 0)
                await _repository.UpsertDeviceSummaryAsync(deviceSummary.Values);            
        }
    }
}
