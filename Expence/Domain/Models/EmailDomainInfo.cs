using Expence.Domain.Constants;

namespace Expence.Domain.Models
{
    public class EmailDomainInfo
    {
        public int Id { get; set; }
        public string Domain { get; set; }
        public bool IsDisposable { get; set; }
        public DateTime CheckedAt { get; set; } = DateTimeConstants.CurrentWestAfricanTime;
        public DateTime? DateModified { get; set; }
    }
}
