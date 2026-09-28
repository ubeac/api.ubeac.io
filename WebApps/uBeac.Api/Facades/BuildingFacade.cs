using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IBuildingFacade : IBaseEntityFacade<Building>
    {
    }

    public class BuildingFacade : BaseEntityFacade<Building>, IBuildingFacade
    {
        public BuildingFacade(IBuildingService buildingService, ISecurityContext securityContext) : base(buildingService, securityContext)
        {
        }
    }
}
