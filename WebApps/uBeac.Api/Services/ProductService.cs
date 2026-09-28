using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IProductService : IBaseEntityService<Product>
    {
    }

    public class ProductService : BaseEntityService<Product>, IProductService
    {
        public ProductService(IProductRepository productRepository) : base(productRepository)
        {
        }
    }
}
