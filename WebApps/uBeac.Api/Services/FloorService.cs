using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IFloorService : IBaseEntityService<Floor>
    {
    }

    public class FloorService : BaseEntityService<Floor>, IFloorService
    {
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDeviceSummaryRepository _deviceSummaryRepository;

        public FloorService(IFloorRepository floorReposiroty,
                            IGatewayRepository gatewayRepository,
                            IDeviceSummaryRepository deviceSummaryRepository,
                            IDeviceRepository deviceRepository) : base(floorReposiroty)
        {
            _gatewayRepository = gatewayRepository;
            _deviceSummaryRepository = deviceSummaryRepository;
            _deviceRepository = deviceRepository;
        }

        public async override Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // deleting floor
            var result = await base.DeleteAsync(id, cancellationToken);
                       
            if (result)
            {
                var tasks = new List<Task>
                {
                    // gateways floor id should be set to null
                    _gatewayRepository.ResetFloorDataAsync(id, cancellationToken),
                    // devicess floor id should be set to null
                    _deviceRepository.ResetFloorDataAsync(id, cancellationToken),
                    // deleting all device summaries 
                    _deviceSummaryRepository.DeleteByFloorIdsAsync(new List<Guid?> { id }, cancellationToken)
                };
                await Task.WhenAll(tasks);
            }

            return result;
        }
    }
}
