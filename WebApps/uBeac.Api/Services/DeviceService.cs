using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IDeviceService : IBaseEntityService<Device>
    {
        Task<List<DeviceSummary>> GetSummaryAsync(Guid teamId, CancellationToken cancellationToken = default);
    }

    public class DeviceService : BaseEntityService<Device>, IDeviceService
    {

        private readonly IDeviceRepository _deviceRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly ISensorDataRepository _sensorDataRepository;
        private readonly IDeviceSummaryRepository _deviceSummaryRepository;

        public DeviceService(IDeviceRepository deviceRepository,
                            ISensorRepository sensorRepository,
                            ISensorDataRepository sensorDataRepository,
                            IDeviceSummaryRepository deviceSummaryRepository) : base(deviceRepository)
        {
            _deviceRepository = deviceRepository;
            _sensorRepository = sensorRepository;
            _sensorDataRepository = sensorDataRepository;
            _deviceSummaryRepository = deviceSummaryRepository;
        }

        public override async Task<List<Device>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var devices = await _deviceRepository.GetByTeamIdAsync(teamId, cancellationToken);
            var sensors = await _sensorRepository.GetByTeamIdAsync(teamId, cancellationToken);
            var devicesDict = devices.ToDictionary(x => x.Id, y => y);
            foreach (var sensor in sensors)
            {
                if (devicesDict.ContainsKey(sensor.DeviceId))
                    devicesDict[sensor.DeviceId].Sensors.Add(sensor);
            }
            return devices;
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {            
            var device = await _deviceRepository.GetByIdAsync(id, cancellationToken);
            var sensors = await _sensorRepository.GetByDeviceIdAsync(id, cancellationToken);
            var sensorIds = sensors.Select(x => x.Id);

            var tasks = new List<Task>
            {
             _sensorRepository.DeleteManyAsync(sensorIds, cancellationToken),// deleting sensors
             _deviceSummaryRepository.DeleteByDeviceIdAsync(id, cancellationToken) // deleting device summary
            };

            await Task.WhenAll(tasks);

            // deleting device
            var result =  await base.DeleteAsync(id, cancellationToken);
            await _sensorDataRepository.DeleteByTeamIdAsync(device.TeamId, sensorIds, cancellationToken);

            return result;
        }

        public async Task<List<DeviceSummary>> GetSummaryAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _deviceSummaryRepository.GetByTeamIdAsync(teamId, cancellationToken);
        }

        public override async Task<Guid> AddAsync(Device model, CancellationToken cancellationToken = default)
        {
            var existingDevice = await _deviceRepository.GetByUidIdAsync(model.TeamId, model.Uid, cancellationToken);
            if (existingDevice is null)
                return await base.AddAsync(model, cancellationToken);

            return Guid.Empty;
        }

        public override async Task<bool> UpdateAsync(Device model, CancellationToken cancellationToken = default)
        {
            var oldModel = await _deviceRepository.GetByIdAsync(model.Id, cancellationToken);
            if (oldModel is null || model.TeamId != oldModel.TeamId)
                return false;

            model.CreateBy = oldModel.CreateBy;
            model.CreateDate = oldModel.CreateDate;
            model.UpdateDate = DateTime.UtcNow;
            model.Uid = oldModel.Uid;

            return await _deviceRepository.UpdateAsync(model, cancellationToken);
        }
    }
}
