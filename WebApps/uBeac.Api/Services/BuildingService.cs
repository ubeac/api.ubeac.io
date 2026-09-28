using uBeac.Models;
using uBeac.Api.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace uBeac.Api.Services
{
    public interface IBuildingService : IBaseEntityService<Building>
    {
    }

    public class BuildingService : BaseEntityService<Building>, IBuildingService
    {
        private readonly IBuildingRepository _buildingReposiroty;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IFloorRepository _floorReposiroty;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDeviceSummaryRepository _deviceSummaryRepository;

        public BuildingService(IBuildingRepository buildingReposiroty,
                                IFloorRepository floorReposiroty,
                                IGatewayRepository gatewayRepository,
                                IDeviceRepository deviceRepository,
                                IDeviceSummaryRepository deviceSummaryRepository) : base(buildingReposiroty)
        {
            _buildingReposiroty = buildingReposiroty;
            _floorReposiroty = floorReposiroty;
            _gatewayRepository = gatewayRepository;
            _deviceSummaryRepository = deviceSummaryRepository;
            _deviceRepository = deviceRepository;
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // deleting building
            var result = await base.DeleteAsync(id, cancellationToken);

            if (result)
            {
                // deleting floors
                var floors = await _floorReposiroty.GetByBuildingIdsAsync(new[] { id }, cancellationToken);
                var floorIds = floors.Select(x => x.Id);

                if (floorIds.Count() > 0)
                {
                    var tasks = new List<Task>
                    { 
                        // deleting child floors
                        _floorReposiroty.DeleteManyAsync(floorIds, cancellationToken),
                        // gateways floor id should be set to null                
                        _gatewayRepository.ResetFloorDataAsync(floorIds.Cast<Guid?>(), cancellationToken),
                        // devices floor id should be set to null                
                        _deviceRepository.ResetFloorDataAsync(floorIds.Cast<Guid?>(), cancellationToken),
                        // deleting all device summaries 
                        _deviceSummaryRepository.DeleteByFloorIdsAsync(floorIds.Cast<Guid?>(), cancellationToken)
                    };

                    await Task.WhenAll(tasks);
                }
            }

            return result;
        }

        public override async Task<List<Building>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var buildings = await _buildingReposiroty.GetByTeamIdAsync(teamId, cancellationToken);
            var floors = await _floorReposiroty.GetByBuildingIdsAsync(buildings.Select(x => x.Id), cancellationToken);
            var buildingsDict = buildings.ToDictionary(x => x.Id, y => y);
            foreach (var floor in floors)
            {
                if (buildingsDict.ContainsKey(floor.BuildingId))
                    buildingsDict[floor.BuildingId].Floors.Add(floor);
            }
            return buildings;
        }
    }
}
