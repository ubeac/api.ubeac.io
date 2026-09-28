using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(FloorConstants.DESCRIPTION)]
    public class FloorController : BaseEntityController<IFloorFacade, Floor, FloorInputModelAdd, FloorInputModelUpdate>
    {
        public FloorController(IFloorFacade floorFacade) : base(floorFacade)
        {
        }

        [SwaggerOperation(Summary = FloorConstants.ADD_SUMMARY, Description = FloorConstants.ADD_DESCRIPTION)]
        public override async Task<ResultSet<Guid>> Add([FromBody, SwaggerParameter("Floor", Required = true)] FloorInputModelAdd floor)
        {
            return await base.Add(floor);
        }

        [SwaggerOperation(Summary = FloorConstants.UPDATE_SUMMARY, Description = FloorConstants.UPDATE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Update([FromBody, SwaggerParameter("Floor", Required = true)] FloorInputModelUpdate floor)
        {
            return await base.Update(floor);
        }

        [SwaggerOperation(Summary = FloorConstants.DELETE_SUMMARY, Description = FloorConstants.DELETE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await base.Remove(id);
        }
    }
}
