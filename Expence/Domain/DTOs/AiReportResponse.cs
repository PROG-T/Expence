namespace Expence.Domain.DTOs
{
    public class AiReportResponse
    {
        public string Report { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Period { get; set; }
    }
}
