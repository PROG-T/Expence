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
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ValidateTransactionOwnershipAttribute>>();

            var transactionReference = context.HttpContext.Request.RouteValues["reference"]?.ToString()
                            ?? context.HttpContext.Request.Query["reference"].ToString();

            logger.LogInformation("Transaction ownership validation. Reference: {Reference}", transactionReference);

            if (string.IsNullOrEmpty(transactionReference))
            {
                logger.LogWarning("Transaction ownership validation failed: Reference is empty or null");
                context.Result = new BadRequestObjectResult(new { message = "Transaction reference is required" });
                return;
            }

            var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContextService>();

            var transaction = await unitOfWork.Transactions.GetByTransactionReferenceAsync(transactionReference);

            if (transaction == null)
            {
                logger.LogWarning("Transaction ownership validation failed: Transaction not found. Reference: {Reference}",
                        transactionReference);
                context.Result = new NotFoundObjectResult(new { message = "Transaction not found" });
                return;
            }

            // Verify user ownership
            var userIdString = userContext.GetUserId().Data;
            if (!long.TryParse(userIdString ?? "0", out var userId))
            {
                logger.LogWarning("Transaction ownership validation failed: Invalid user ID format. UserId: {UserId}",
                    userIdString);
                context.Result = new ForbidResult();
                return;
            }

            if (transaction.UserId != userId)
            {
                logger.LogWarning(
                        "Transaction ownership validation failed: User does not own this transaction. Reference: {Reference}, TransactionOwnerId: {TransactionOwnerId}, RequestedUserId: {RequestedUserId}",
                        transactionReference, transaction.UserId, userId);
                context.Result = new ForbidResult();
                return;
            }
            logger.LogInformation(
                    "Transaction ownership validation successful. Reference: {Reference}, UserId: {UserId}",
                    transactionReference, userId);
        
    }
    }
}
