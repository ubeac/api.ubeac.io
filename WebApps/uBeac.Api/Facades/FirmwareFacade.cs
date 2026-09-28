using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IFirmwareFacade : IBaseEntityFacade<Firmware>
    {
    }

    public class FirmwareFacade : BaseEntityFacade<Firmware>, IFirmwareFacade
    {
        public FirmwareFacade(IFirmwareService firmwareService, ISecurityContext securityContext) : base(firmwareService, securityContext)
        {
        }
    }
}
