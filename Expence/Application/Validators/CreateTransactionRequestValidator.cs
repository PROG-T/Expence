using Expence.Domain.DTOs;
using FluentValidation;

namespace Expence.Application.Validators
{
    public class CreateTransactionRequestValidator:AbstractValidator<CreateTransactionRequest>
    {
        public CreateTransactionRequestValidator()
        {
            RuleFor(x => x.Amount)
                .NotEmpty()
                    .WithMessage("Amount is required")
                .GreaterThan(0)
                    .WithMessage("Amount must be greater than zero")
             .LessThanOrEqualTo(999999999.99m).WithMessage("Amount exceeds maximum allowed value");


            RuleFor(x => x.Category)
                .NotEmpty()
                    .WithMessage("Category is required")
                .Length(2, 100)
                    .WithMessage("Category must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_]+$")
                    .WithMessage("Category contains invalid characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500)
                    .WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Type)
                .NotEmpty()
                    .WithMessage("Transaction type is required")
                .IsInEnum()
                    .WithMessage("Invalid transaction type");

            RuleFor(x => x.Category)
               .NotEmpty().WithMessage("Category is required")
               .MaximumLength(100).WithMessage("Category cannot exceed 100 characters");
        }
    }
}
