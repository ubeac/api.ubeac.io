using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IProductFacade : IBaseEntityFacade<Product>
    {
    }

    public class ProductFacade : BaseEntityFacade<Product>, IProductFacade
    {
        public ProductFacade(IProductService productService, ISecurityContext securityContext) : base(productService, securityContext)
        {
        }
    }
}
