namespace OpenAI_POC.Service
{
    public interface IOpenAIService
    {
        Task<string> CreateChatResponse(string input);
        Task<string> AskSalesQuestionAsync(string userQuestion);
        Task<List<string>> GenerateLogoImagesAsync(string prompt, string model);
        ReadOnlyMemory<float> GetEmbeddingVector(string input);
    }
}
