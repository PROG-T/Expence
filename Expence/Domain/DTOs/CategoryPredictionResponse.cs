namespace Expence.Domain.DTOs
{
    public class CategoryPredictionResponse
    {
        public string PredictedCategory { get; set; }
        public float Confidence { get; set; }
        public bool IsValid { get; set; }
    }
}
