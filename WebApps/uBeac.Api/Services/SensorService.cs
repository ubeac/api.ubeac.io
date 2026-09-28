using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;
using System.Linq;
using System.Diagnostics;

namespace uBeac.Api.Services
{
    public interface ISensorService : IBaseEntityService<Sensor>
    {
        Task<List<SensorData>> GetDataAsync(Guid teamId, IEnumerable<Guid> deviceIds, IEnumerable<Guid> sensorIds, DateTime fromDate, DateTime toDate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);        
    }

    public class SensorService : BaseEntityService<Sensor>, ISensorService
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly ISensorDataRepository _sensorDataRepository;
        private readonly IDeviceRepository _deviceRepository;

        public SensorService(ISensorRepository sensorRepository,
                            ISensorDataRepository sensorDataRepository,
                            IDeviceRepository deviceRepository) : base(sensorRepository)
        {
            _sensorRepository = sensorRepository;
            _sensorDataRepository = sensorDataRepository;
            _deviceRepository = deviceRepository;
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var sensor = await base.GetByIdAsync(id, cancellationToken);

            if (sensor is null)
                return await Task.FromResult(false);

            var result = await base.DeleteAsync(id, cancellationToken);
            
            await _sensorDataRepository.DeleteByTeamIdAsync(sensor.TeamId, new Guid[] { id }, cancellationToken);

            return result;
        }
        
        public async Task<List<SensorData>> GetDataAsync(Guid teamId, IEnumerable<Guid> deviceIds, IEnumerable<Guid> sensorIds, DateTime fromDate, DateTime toDate, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        {
            var availableDevices = await _deviceRepository.GetByTeamIdAsync(teamId, cancellationToken);
            var availableDeviceIds = availableDevices.Select(x => x.Id);

            if (deviceIds.Count() > 0)
                deviceIds = deviceIds.Where(x => availableDeviceIds.Contains(x));
            else
                deviceIds = availableDeviceIds;

            var availableSensors = await _sensorRepository.GetByDeviceIdsAsync(deviceIds, cancellationToken);
            var availableSensorIds = availableSensors.Select(x => x.Id);

            if (sensorIds.Count() > 0)
                sensorIds = sensorIds.Where(x => availableSensorIds.Contains(x));
            else
                sensorIds = availableSensorIds;

            if (sensorIds.Count() == 0)
                return await Task.FromResult(new List<SensorData>());

            return await _sensorDataRepository.GetDataAsync(teamId, sensorIds, fromDate, toDate, pageSize, pageNumber, cancellationToken);
        }

        public override async Task<Guid> AddAsync(Sensor model, CancellationToken cancellationToken = default)
        {
            var existingSensor = await _sensorRepository.GetByUidIdAsync(model.DeviceId, model.Uid, cancellationToken);
            if (existingSensor is null)
                return await base.AddAsync(model, cancellationToken);

            return Guid.Empty;
        }

        public override async Task<bool> UpdateAsync(Sensor model, CancellationToken cancellationToken = default)
        {
            var oldModel = await _sensorRepository.GetByIdAsync(model.Id, cancellationToken);
            if (oldModel is null || model.TeamId != oldModel.TeamId)
                return false;

            model.CreateBy = oldModel.CreateBy;
            model.CreateDate = oldModel.CreateDate;
            model.UpdateDate = DateTime.UtcNow;
            model.Uid = oldModel.Uid;
            model.Schema = oldModel.Schema;

            return await _sensorRepository.UpdateAsync(model, cancellationToken);
        }
    }
}
