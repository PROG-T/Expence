using Expence.Domain.Enum;

namespace Expence.Domain.DTOs
{
    public class CreateTransactionDTO
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; }
    }
}
