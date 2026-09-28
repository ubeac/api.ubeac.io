using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IDashboardRepository: IBaseEntityRepository<Dashboard>
    {
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class DashboardRepository : BaseEntityRepository<Dashboard>, IDashboardRepository
    {
        public DashboardRepository(MainDatabase database) : base(database)
        {
        }
    }
}
