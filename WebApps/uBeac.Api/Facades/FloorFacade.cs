using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IFloorFacade : IBaseEntityFacade<Floor>
    {
    }

    public class FloorFacade : BaseEntityFacade<Floor>, IFloorFacade
    {
        public FloorFacade(IFloorService floorService, ISecurityContext securityContext) : base(floorService, securityContext)
        {
        }
    }
}
