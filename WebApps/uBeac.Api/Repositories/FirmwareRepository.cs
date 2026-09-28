using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IFirmwareRepository : IBaseEntityRepository<Firmware>
    {
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class FirmwareRepository : BaseEntityRepository<Firmware>, IFirmwareRepository
    {
        public FirmwareRepository(MainDatabase database) : base(database)
        {
        }
    }
}
