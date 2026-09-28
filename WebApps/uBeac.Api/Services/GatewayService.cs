using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IGatewayService : IBaseEntityService<Gateway>
    {
        Task<List<GatewayData>> GetData(Guid teamId, Guid gatewayId, DateTime fromDate, DateTime toDate, int pageSize, int pageCount, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid teamId, string url, CancellationToken cancellationToken = default);
    }

    public class GatewayService : BaseEntityService<Gateway>, IGatewayService
    {
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IDeviceSummaryRepository _deviceSummaryRepository;
        private readonly IGatewayDataRepository _gatewayDataRepository;
        private readonly ISensorDataRepository _sensorDataRepository;

        public GatewayService(IGatewayRepository gatewayRepository,
                              IDeviceSummaryRepository deviceSummaryRepository,
                              IGatewayDataRepository gatewayDataRepository,
                              ISensorDataRepository sensorDataRepository) : base(gatewayRepository)
        {
            _gatewayRepository = gatewayRepository;
            _deviceSummaryRepository = deviceSummaryRepository;
            _gatewayDataRepository = gatewayDataRepository;
            _sensorDataRepository = sensorDataRepository;
        }

        public async override Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // fetching gateway
            var gateway = await _gatewayRepository.GetByIdAsync(id, cancellationToken);
            if (gateway is null)
                return false;

            var tasks = new List<Task>
            { 
            // deleting gateway data
            _gatewayDataRepository.DeleteByTeamIdAsync(gateway.TeamId, id, cancellationToken),

            // deleting sensor data
            _sensorDataRepository.DeleteByTeamIdAsync(gateway.TeamId, id, cancellationToken),

            // deleting device summaries
            _deviceSummaryRepository.DeleteByGatewayIdAsync(id, cancellationToken),
            };

            await Task.WhenAll(tasks);

            // deleting gateway
            return await base.DeleteAsync(id, cancellationToken);
        }

        public override async Task<Guid> AddAsync(Gateway model, CancellationToken cancellationToken = default)
        {
            if (model.Url.StartsWith("momentaj", true, CultureInfo.InvariantCulture) || model.Url.StartsWith("ubeac", true, CultureInfo.InvariantCulture))
                return Guid.Empty;

            var gateway = await _gatewayRepository.GetByUrlAsync(model.TeamId, model.Url, cancellationToken);
            if (gateway != null)
                return Guid.Empty;

            return await base.AddAsync(model, cancellationToken);
        }

        public override async Task<bool> UpdateAsync(Gateway model, CancellationToken cancellationToken = default)
        {
            var oldModel = await _gatewayRepository.GetByIdAsync(model.Id, cancellationToken);
            if (oldModel is null || model.TeamId != oldModel.TeamId || model.Url.StartsWith("momentaj", true, CultureInfo.InvariantCulture) || model.Url.StartsWith("ubeac", true, CultureInfo.InvariantCulture))
                return false;

            var gateway = await _gatewayRepository.GetByUrlAsync(model.TeamId, model.Url, cancellationToken);

            if (gateway != null && gateway.Id != model.Id)
                return false;

            model.CreateBy = oldModel.CreateBy;
            model.CreateDate = oldModel.CreateDate;
            model.UpdateDate = DateTime.UtcNow;
            model.LastRequestDate = oldModel.LastRequestDate;
            model.RequestCount = oldModel.RequestCount;

            return await _gatewayRepository.UpdateAsync(model, cancellationToken);
        }

        public Task<List<GatewayData>> GetData(Guid teamId, Guid gatewayId, DateTime fromDate, DateTime toDate, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            return _gatewayDataRepository.GetDataAsync(teamId, gatewayId, fromDate, toDate, pageSize, pageCount, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid teamId, string url, CancellationToken cancellationToken = default)
        {
            if (url.ToLower().StartsWith("momentaj") || url.ToLower().StartsWith("ubeac"))
                return true;

            var gateway = await _gatewayRepository.GetByUrlAsync(teamId, url, cancellationToken);
            return gateway != null;
        }
    }
}
