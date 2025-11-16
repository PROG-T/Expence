namespace Expence.Domain.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
