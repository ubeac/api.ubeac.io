using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IFirmwareService : IBaseEntityService<Firmware>
    {
    }
    public class FirmwareService : BaseEntityService<Firmware>, IFirmwareService
    {
        public FirmwareService(IFirmwareRepository firmwareRepository) : base(firmwareRepository)
        {
        }
    }
}
