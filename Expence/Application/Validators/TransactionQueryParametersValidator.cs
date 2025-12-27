using Expence.Domain.DTOs;
using FluentValidation;

namespace Expence.Application.Validators
{
    public class TransactionQueryParametersValidator : AbstractValidator<TransactionQueryParameters>
    {
        public TransactionQueryParametersValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                    .WithMessage("Page must be at least 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                    .WithMessage("PageSize must be at least 1")
                .LessThanOrEqualTo(100)
                    .WithMessage("PageSize cannot exceed 100");

            RuleFor(x => x.Category)
                .MaximumLength(100)
                    .WithMessage("Category cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Category));

            RuleFor(x => x.FromDate)
                .Must((x, fromDate) => !fromDate.HasValue || fromDate <= x.ToDate)
                    .WithMessage("FromDate must be less than or equal to ToDate")
                .When(x => x.FromDate.HasValue);

            RuleFor(x => x.ToDate)
                .Must((x, toDate) => !toDate.HasValue || toDate <= DateTime.UtcNow)
                    .WithMessage("ToDate cannot be in the future")
                .When(x => x.ToDate.HasValue);
        }
    }
}
