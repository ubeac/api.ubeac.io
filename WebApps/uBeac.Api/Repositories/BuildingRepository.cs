using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IBuildingRepository : IBaseEntityRepository<Building>
    {
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class BuildingReposiroty : BaseEntityRepository<Building>, IBuildingRepository
    {
        public BuildingReposiroty(MainDatabase database) : base(database)
        {
        }
    }
}
