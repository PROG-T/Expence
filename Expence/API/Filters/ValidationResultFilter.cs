using Expence.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Expence.API.Filters
{
    public class ValidationResultFilter: IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if ModelState contains validation errors
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new BaseResponse<object>(false, "One or more validation errors occurred.")
                {
                    Data = new { errors = errors }
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No operation needed after action execution
        }
    }
}
