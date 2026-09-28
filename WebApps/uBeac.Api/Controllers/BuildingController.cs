using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(BuildingConstants.DESCRIPTION)]
    public class BuildingController : BaseEntityController<IBuildingFacade, Building, BuildingInputModelAdd, BuildingInputModelUpdate>
    {
        public BuildingController(IBuildingFacade buildingFacade) : base(buildingFacade)
        {
        }

        [SwaggerOperation(Summary = BuildingConstants.DELETE_SUMMARY, Description = BuildingConstants.DELETE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await base.Remove(id);
        }
    }
}
