using FluentValidation;

namespace Expence.Application.Validators
{
    public static class CustomValidationExtensions
    {
        public static IRuleBuilderOptionsConditions<T, decimal> PrecisionScale<T>(
            this IRuleBuilder<T, decimal> ruleBuilder,
            int precision,
            int scale,
            bool ignoreTrailingZeros = true)
        {
            return ruleBuilder.Custom((value, context) =>
            {
                var decimalValue = value;
                var parts = decimalValue.ToString("G").Split('.');

                if (parts.Length > 1 && parts[1].Length > scale)
                {
                    context.AddFailure(
                        $"The decimal value must have at most {scale} decimal places");
                }
            });
        }
    }
}
