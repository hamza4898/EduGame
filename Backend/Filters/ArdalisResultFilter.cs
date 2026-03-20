using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ardalis.Result;

namespace EduGame.Filters
{
    public class ResultValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            if (executedContext.Result is ObjectResult objectResult && objectResult.Value is Ardalis.Result.IResult result)
            {
                if (!(result.Status == ResultStatus.Ok))
                {
                    var message = result.Errors.FirstOrDefault() 
                        ?? result.ValidationErrors.FirstOrDefault()?.ErrorMessage;

                    executedContext.Result = result.Status switch
                    {
                        ResultStatus.Invalid => new BadRequestObjectResult(new { error = message }),
                        ResultStatus.NotFound => new NotFoundObjectResult(new { error = message }),
                        ResultStatus.Conflict => new ConflictObjectResult(new { error = message }),
                        ResultStatus.Unauthorized => new ObjectResult(new { error = message }) { StatusCode = 401 },
                        ResultStatus.Forbidden => new ObjectResult(new { error = message }) { StatusCode = 403 },
                        _ => new BadRequestObjectResult(new { error = message }) 
                    };   
                }
                else
                    objectResult.Value = (result as dynamic).Value;
            }   
        }
    }
}