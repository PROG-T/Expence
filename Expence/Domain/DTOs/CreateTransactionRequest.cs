using Expence.Domain.Constants;
using Expence.Domain.Enum;
using Expence.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Expence.Domain.DTOs
{
    public class CreateTransactionRequest
    {
        public decimal Amount {  get; set; }
        public string Description {  get; set; }
        public string Category {  get; set; }
        public TransactionType Type {  get; set; }
    }
}
