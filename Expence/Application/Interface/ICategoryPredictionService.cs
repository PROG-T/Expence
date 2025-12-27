using Expence.Domain.Enum;

namespace Expence.Application.Interface
{
    public interface ICategoryPredictionService
    {
        Task<string> PredictCategoryAsync(string description, TransactionType type);

    }
}
