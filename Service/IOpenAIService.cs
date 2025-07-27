using OpenAI_POC.Models;

namespace OpenAI_POC.Service
{
    public interface IOpenAIService
    {
        Task<string> CreateChatResponse(string input);
        Task<string> CreateChatResponseAsync(List<ChatMessage> chatHistory);
        Task<string> AskSalesQuestionAsync(string userQuestion);
        Task<List<string>> GenerateLogoImagesAsync(string prompt, string model);
        Task<string> GenerateColorPaletteAsync(string description);
        Task<string> GenerateMarkdownArticleAsync(MarkdownArticleRequest request);
        ReadOnlyMemory<float> GetEmbeddingVector(string input);
    }
}
