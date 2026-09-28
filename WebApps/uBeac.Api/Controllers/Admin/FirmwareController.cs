using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    public class FirmwareController : AdminBaseEntityController<Firmware, FirmwareInputModel>
    {
        public FirmwareController(IFirmwareFacade firmwareFacade) : base(firmwareFacade)
        {
        }
    }
}
