using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface ISensorFacade : IBaseEntityFacade<Sensor>
    {
        Task<ResultSet<List<SensorData>>> GetDataAsync(Guid teamId, IEnumerable<Guid> deviceIds, IEnumerable<Guid> sensorIds, DateTime fromDate, DateTime toDate, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    }
    public class SensorFacade : BaseEntityFacade<Sensor>, ISensorFacade
    {
        private readonly ISensorService _sensorService;
        public SensorFacade(ISensorService sensorService, ISecurityContext securityContext) : base(sensorService, securityContext)
        {
            _sensorService = sensorService;
        }
        
        public async Task<ResultSet<List<SensorData>>> GetDataAsync(Guid teamId, IEnumerable<Guid> deviceIds, IEnumerable<Guid> sensorIds, DateTime fromDate, DateTime toDate, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        {
            if (!SecurityContext.HasAccess<Team>(AccessLevels.View, teamId))
                return new ResultSet<List<SensorData>>(ResponseCodes.UnAuthorized);

            return new ResultSet<List<SensorData>>(await _sensorService.GetDataAsync(teamId, deviceIds, sensorIds, fromDate, toDate, pageSize, pageNumber, cancellationToken));
        }
    }
}
