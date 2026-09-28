using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uBeac.Web.Filters;

namespace uBeac.Api.Controllers
{
    [Route("[controller]/[action]/")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "ADMINS")]
    [TypeFilter(typeof(ResultSetResponseCodeFilter))]
    [TypeFilter(typeof(ModelStateValidationAsyncActionFilter))]
    public abstract class AdminBaseController
    {

    }
}
