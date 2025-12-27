using Expence.Domain.Enum;

namespace Expence.Domain.DTOs
{
    public class TransactionQueryParameters
    {
       
            public string? Category { get; set; }
            public TransactionType? Type { get; set; }  
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        
    }
    public class TransactionQueryRequest
    {
       
        public long UserId { get; set; }
        public string? Category { get; set; }
            public TransactionType? Type { get; set; }  
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        
    }

}
