using uBeac.Api.InputModels;
using uBeac.Models;
using uBeac.Api.Facades;

namespace uBeac.Api.Controllers.Admin
{
    public class ProductController : AdminBaseEntityController<Product, ProductInputModel>
    {
        public ProductController(IProductFacade productFacade) : base(productFacade)
        {
        }
    }
}
