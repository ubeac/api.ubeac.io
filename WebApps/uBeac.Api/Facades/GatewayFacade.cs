using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IGatewayFacade : IBaseEntityFacade<Gateway>
    {
        Task<ResultSet<List<GatewayData>>> GetData(Guid gatewayId, DateTime fromDate, DateTime toDate, int pageSize, int pageCount, CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> ExistsAsync(Guid teamId, string url, CancellationToken cancellationToken = default);
    }

    public class GatewayFacade : BaseEntityFacade<Gateway>, IGatewayFacade
    {
        private readonly IGatewayService _gatewayService;
        public GatewayFacade(IGatewayService gatewayService, ISecurityContext securityContext) : base(gatewayService, securityContext)
        {
            _gatewayService = gatewayService;
        }

        public async Task<ResultSet<List<GatewayData>>> GetData(Guid gatewayId, DateTime fromDate, DateTime toDate, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            var gateway = (await base.GetByIdAsync(gatewayId, cancellationToken)).Data;

            if (gateway is null)
                return new ResultSet<List<GatewayData>>(new List<GatewayData>());

            if (!SecurityContext.HasAccess<Team>(AccessLevels.View, gateway.TeamId))
                return new ResultSet<List<GatewayData>>(new List<GatewayData>(), ResponseCodes.UnAuthorized);

            var result = await _gatewayService.GetData(gateway.TeamId, gatewayId, fromDate, toDate, pageSize, pageCount, cancellationToken);
            return new ResultSet<List<GatewayData>>(result);
        }

        public async Task<ResultSet<bool>> ExistsAsync(Guid teamId, string url, CancellationToken cancellationToken = default)
        {
            return new ResultSet<bool>(await _gatewayService.ExistsAsync(teamId, url, cancellationToken));
        }
    }
}
