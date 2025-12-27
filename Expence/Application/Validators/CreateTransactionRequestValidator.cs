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
                    .WithMessage("Amount must be greater than zero");
               

            RuleFor(x => x.Category)
                .NotEmpty()
                    .WithMessage("Category is required")
                .Length(2, 100)
                    .WithMessage("Category must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_]+$")
                    .WithMessage("Category contains invalid characters");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                    .WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Type)
                .NotEmpty()
                    .WithMessage("Transaction type is required")
                .IsInEnum()
                    .WithMessage("Invalid transaction type");
        }
    }
}
