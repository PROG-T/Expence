namespace Expence.Application.Interface
{
    public interface IExpenseSummaryGeneratorService
    {
        Task<string> GenerateMonthlyReportAsync(long userId);    }
}
