using Expence.Application.Interface;
using Expence.Infrastructure.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Expence.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateTransactionOwnershipAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var transactionReference = context.HttpContext.Request.RouteValues["reference"]?.ToString()
                            ?? context.HttpContext.Request.Query["reference"].ToString();

            if (string.IsNullOrEmpty(transactionReference))
            {
                context.Result = new BadRequestObjectResult(new { message = "Transaction reference is required" });
                return;
            }

            var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContextService>();

            var transaction = await unitOfWork.Transactions.GetByTransactionReferenceAsync(transactionReference);

            if (transaction == null)
            {
                context.Result = new NotFoundObjectResult(new { message = "Transaction not found" });
                return;
            }

            // Verify user ownership
            var userId = long.Parse(userContext.GetUserId() ?? "0");
            if (transaction.UserId != userId)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
