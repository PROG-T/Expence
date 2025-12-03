using Expence.Domain.Enum;

namespace Expence.Domain.DTOs
{
    public class UpdateTransactionRequest
    {
        public string  transactionReference {  get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; }
    }
}
