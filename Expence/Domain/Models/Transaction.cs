using Expence.Domain.Constants;
using Expence.Domain.Enum;

namespace Expence.Domain.Models
{
    public class Transaction
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public TransactionType Type { get; set; } // "income/credit" or "expense/debit"
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string TransactionReference { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTimeConstants.CurrentWestAfricanTime;
        public DateTime? ModifiedAt { get; set; } 
    }


}
