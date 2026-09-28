using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace uBeac.Web.Filters
{
    public class ResultSetResponseCodeFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result != null && typeof(ObjectResult) == context.Result.GetType())
            {
                var objectResult = (ObjectResult)context.Result;
                if (typeof(IResultSet).IsAssignableFrom(objectResult.Value.GetType()))
                {
                    var result = (IResultSet)objectResult.Value;
                    context.HttpContext.Response.StatusCode = (int)result.Code;
                }
            }
        }
    }
}
