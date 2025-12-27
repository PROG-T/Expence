using Expence.Domain.Enum;

namespace Expence.Domain.DTOs
{
    public class PredictCategoryRequest
    {
        public string Description { get; set; } = string.Empty;
        public  TransactionType Type { get; set; }
    }
}
