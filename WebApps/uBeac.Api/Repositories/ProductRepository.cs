using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IProductRepository : IBaseEntityRepository<Product>
    {
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class ProductRepository : BaseEntityRepository<Product>, IProductRepository
    {
        public ProductRepository(MainDatabase database) : base(database)
        {
        }
    }
}
