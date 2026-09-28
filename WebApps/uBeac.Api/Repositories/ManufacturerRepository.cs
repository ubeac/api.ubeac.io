using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IManufacturerRepository : IBaseEntityRepository<Manufacturer>
    {
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class ManufacturerRepository : BaseEntityRepository<Manufacturer>, IManufacturerRepository
    {
        public ManufacturerRepository(MainDatabase database) : base(database)
        {
        }
    }
}
