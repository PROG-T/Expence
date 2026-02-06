using Expence.Domain.DTOs;
using FluentValidation;

namespace Expence.Application.Validators
{
    public class ResendVerificationEmailRequestValidator: AbstractValidator<ResendVerificationEmailRequest>
    {
        public ResendVerificationEmailRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be valid");
        }
    }
}
