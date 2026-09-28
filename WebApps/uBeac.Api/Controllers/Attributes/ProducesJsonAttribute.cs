using Microsoft.AspNetCore.Mvc;

namespace uBeac.Api.Controllers
{
    public class ProducesJsonAttribute : ProducesAttribute
    {
        public ProducesJsonAttribute() : base("application/json")
        {

        }
    }
}
