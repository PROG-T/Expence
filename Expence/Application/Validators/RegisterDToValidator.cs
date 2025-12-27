using Expence.Domain.DTOs;
using FluentValidation;

namespace Expence.Application.Validators
{
    public class RegisterDToValidator:AbstractValidator<RegisterDTo>
    {
        public RegisterDToValidator()
        {
            RuleFor(x => x.Email)
               .NotEmpty()
                   .WithMessage("Email is required")
               .EmailAddress()
                   .WithMessage("Email format is invalid")
               .MaximumLength(255)
                   .WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Password is required")
                .MinimumLength(8)
                    .WithMessage("Password must be at least 8 characters")
                .MaximumLength(256)
                    .WithMessage("Password cannot exceed 256 characters")
                .Matches(@"^(?=.*[A-Z])")
                    .WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"^(?=.*[a-z])")
                    .WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"^(?=.*\d)")
                    .WithMessage("Password must contain at least one digit")
                .Matches(@"^(?=.*[^\w\s])")
                    .WithMessage("Password must contain at least one special character")
                .Must(password => !HasCommonPatterns(password))
                    .WithMessage("Password is too common, please choose a stronger password");
        }

        private bool HasCommonPatterns(string password)
        {
            var commonPatterns = new[] { "123456", "password", "qwerty", "abc123", "admin", "letmein" };
            return commonPatterns.Any(p => password.ToLower().Contains(p));
        }
    }
}
