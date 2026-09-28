using Microsoft.AspNetCore.Mvc;
using NetTools;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(GatewayConstants.DESCRIPTION)]
    public class GatewayController : BaseEntityController<IGatewayFacade, Gateway, GatewayInputModelAdd, GatewayInputModelUpdate>
    {
        private readonly IGatewayFacade _gatewayFacade;

        public GatewayController(IGatewayFacade gatewayFacade) : base(gatewayFacade)
        {
            _gatewayFacade = gatewayFacade;
        }

        [HttpGet]
        [ProducesJson]
        [SwaggerOperation(Summary = GatewayConstants.GETDATA_SUMMARY, Description = GatewayConstants.GETDATA_DESCRIPTION)]
        public async Task<ResultSet<List<GatewayData>>> GetData(Guid gatewayId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1,
                                                                int pageSize = 20)
        {
            var FromDate = fromDate != null ? (DateTime)fromDate : DateTime.UtcNow.AddDays(-1);
            var ToDate = toDate != null ? (DateTime)toDate : DateTime.UtcNow;

            if (gatewayId == Guid.Empty)
                return new ResultSet<List<GatewayData>>(new List<GatewayData>());

            return await _gatewayFacade.GetData(gatewayId, FromDate, ToDate, pageSize, pageNumber);
        }

        [SwaggerOperation(Summary = GatewayConstants.ADD_SUMMARY, Description = GatewayConstants.ADD_DESCRIPTION)]
        public override async Task<ResultSet<Guid>> Add([FromBody, SwaggerParameter("Gateway", Required = true)] GatewayInputModelAdd gateway)
        {
            if (!ValidIps(gateway.Security.IpRestriction.AllowedIps) || !ValidIps(gateway.Security.IpRestriction.DeniedIps))
                return new ResultSet<Guid>(Guid.Empty, ResponseCodes.Forbidden);

            return await base.Add(gateway);
        }

        [SwaggerOperation(Summary = GatewayConstants.UPDATE_SUMMARY, Description = GatewayConstants.UPDATE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Update([FromBody, SwaggerParameter("Gateway", Required = true)] GatewayInputModelUpdate gateway)
        {
            if (!ValidIps(gateway.Security.IpRestriction.AllowedIps) || !ValidIps(gateway.Security.IpRestriction.DeniedIps))
                return new ResultSet<bool>(false, ResponseCodes.Forbidden);

            return await base.Update(gateway);
        }

        [SwaggerOperation(Summary = GatewayConstants.DELETE_SUMMARY, Description = GatewayConstants.DELETE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await base.Remove(id);
        }

        [HttpGet("{url}")]
        [SwaggerOperation(Summary = GatewayConstants.EXISTS_SUMMARY, Description = GatewayConstants.EXISTS_DESCRIPTION)]
        public async Task<ResultSet<bool>> Exists(Guid teamId, string url)
        {
            return await _gatewayFacade.ExistsAsync(teamId, url);
        }

        private bool ValidIps(List<string> ips)
        {            
            if (ips.Count > 0)
            {
                foreach (var ip in ips)
                {
                    if (!IPAddressRange.TryParse(ip, out IPAddressRange ipRange))
                        return false;

                    var a = ipRange;
                }
            }
            return true;
        }

    }
}
