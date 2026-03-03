using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentResults;

namespace EduGame.Filters
{
    public class ResultValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            if (executedContext.Result is ObjectResult obj && obj.Value is IResultBase fluentResult)
            {
                if (fluentResult.IsFailed)
                {
                    var message = fluentResult.Errors.First().Message;

                    executedContext.Result = new BadRequestObjectResult(new
                    {
                        error = message
                    });
                }   
                else
                {
                    var data = (fluentResult as dynamic).Value;

                    obj.Value = data;
                } 
            }
        }
    }
}