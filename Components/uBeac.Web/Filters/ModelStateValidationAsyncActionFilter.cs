using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace uBeac.Web.Filters
{
    public class ModelStateValidationAsyncActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var req = context.HttpContext.Request.Path;

            if (context.ModelState.IsValid)
            {
                var resultContext = await next();
            }
            else
            {
                var result = new ResultSet<string>
                {
                    Code = ResponseCodes.BadRequest
                };

                foreach (string key in context.ModelState.Keys)
                {
                    foreach (var error in context.ModelState[key].Errors)
                    {
                        result.AddError(new Error(ErrorCodes.MODEL_VALIDATION_ERROR, key + "," + error.ErrorMessage));
                    }
                }
                context.Result = new BadRequestObjectResult(result);

            }
        }
    }
}
