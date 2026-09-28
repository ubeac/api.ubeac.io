using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using uBeac.Web.Filters;

namespace uBeac.Api.Controllers
{
    [Route("[controller]/[action]/")]
    [Authorize]
    [TypeFilter(typeof(ResultSetResponseCodeFilter))]
    [TypeFilter(typeof(ModelStateValidationAsyncActionFilter))]
    [ProducesJson]
    [SwaggerResponse(200, "OK")]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Forbidden")]
    [SwaggerResponse(404, "Not Found")]
    [SwaggerResponse(500, "Unhandled Exception")]
    public abstract class BaseController
    {
    }
}
