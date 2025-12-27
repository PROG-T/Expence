using Expence.Domain.DTOs;
using FluentValidation;

namespace Expence.Application.Validators
{
    public class LoginDTOValidator: AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Password is required")
                .MinimumLength(8)
                    .WithMessage("Password must be at least 8 characters")
                .MaximumLength(256)
                    .WithMessage("Password cannot exceed 256 characters");
        }
    }
}
