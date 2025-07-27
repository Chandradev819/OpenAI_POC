using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Images;
using OpenAI_POC.Models;
using System.ClientModel;

namespace OpenAI_POC.Service
{
    public class OpenAIService : IOpenAIService
    {
        private readonly string _apiKey;
        private readonly EmbeddingClient _embeddingClient;
        private readonly ChatClient _client;

        public OpenAIService(IConfiguration config)
        {
            _apiKey = config["OpenAI:openai_key"] ?? string.Empty;
            _embeddingClient = new EmbeddingClient("text-embedding-3-small", _apiKey);
            _client = new ChatClient("gpt-4", _apiKey);

        }

        public async Task<string> CreateChatResponse(string input)
        {
            // Create a ChatClient instance with the desired model and API key
            ChatClient client = new("gpt-4.1", _apiKey);

            // Send the input prompt and get the response
            ChatCompletion completion = await client.CompleteChatAsync(input);

            // Return the first response from the content array
            return completion.Content[0].Text;
        }


        // Mock Json data. It could be from a file or a database
        private static readonly string MockJsonData = """
        {
          "description": "This document contains the sale history data for Contoso products.",
          "sales": [
            { "month": "January", "by_product": { "113043": 15, "113045": 12, "113049": 2 } },
            { "month": "February", "by_product": { "113045": 22 } },
            { "month": "March", "by_product": { "113045": 16, "113055": 5 } }
          ]
        }
        """;

        public async Task<string> AskSalesQuestionAsync(string userQuestion)
        {
            ChatClient client = new("gpt-4.1", _apiKey);
            var prompt = $"""
                You are a helpful assistant with access to product sales history.

                Context:
                {MockJsonData}

                Question: {userQuestion}

                Only use the context above to answer. Respond in a helpful tone.
                """;

            ChatCompletion completion = await client.CompleteChatAsync(prompt);
            return completion.Content[0].Text;
        }
        public async Task<string> GenerateColorPaletteAsync(string description)
        {
            ChatClient client = new("gpt-4.1", _apiKey);
            var prompt = $"""
            Given the description: "{description}", generate a color palette of 5 hex codes that represent this theme.
            Only return a JSON array of 5 strings like this:
            ["#FF5733", "#C70039", "#900C3F", "#581845", "#1C1C1C"]
            """;
            ChatCompletion completion = await client.CompleteChatAsync(prompt);
            return completion.Content[0].Text;
        }

        public async Task<string> CreateChatResponseAsync(List<Models.ChatMessage> chatHistory)
        {
            if (chatHistory == null || !chatHistory.Any())
                throw new ArgumentException("Chat history cannot be null or empty");

            var messages = chatHistory.Select<Models.ChatMessage, OpenAI.Chat.ChatMessage>(m =>
            {
                string role = m.Role.ToLower();
                string content = m.Content ?? string.Empty;
                return role switch
                {
                    "you" => OpenAI.Chat.ChatMessage.CreateUserMessage(content),
                    "ai" => OpenAI.Chat.ChatMessage.CreateAssistantMessage(content),
                    _ => OpenAI.Chat.ChatMessage.CreateSystemMessage(content)
                };
            }).ToList();

            try
            {
                var options = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 1000, // Use MaxOutputTokenCount instead of MaxTokens
                    Temperature = 0.7f
                };

                ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages, options);

                if (result?.Value?.Content?.Any() != true)
                    throw new Exception("No content in API response");

                return result.Value.Content[0].Text ?? throw new Exception("Empty response content");
            }
            catch (Exception ex)
            {
                throw new Exception($"OpenAI API error: {ex.Message}", ex);
            }
        }


        public async Task<string> GenerateMarkdownArticleAsync(MarkdownArticleRequest request)
        {
            var prompt = $"""
        You are a professional content writer.

        Write a complete blog article in **Markdown format** based on the following:

        - Title: {request.Title}
        - Target Audience: {request.Audience}

        Use Markdown headers, bullet points, and examples.
        """;

            return await CreateChatResponse(prompt);
        }

        public ReadOnlyMemory<float> GetEmbeddingVector(string input)
        {
            var result = _embeddingClient.GenerateEmbedding(input);
            return result.Value.ToFloats();
        }
        public async Task<List<string>> GenerateLogoImagesAsync(string prompt, string model)
        {
            int numImages = (model == "dall-e-3") ? 1 : 4;

            ImageClient client = new(model, _apiKey);
            GeneratedImageCollection generatedImages = await client.GenerateImagesAsync(prompt, numImages);

            List<string> imageUrls = new List<string>();
            foreach (var image in generatedImages)
            {
                imageUrls.Add(image.ImageUri.ToString());
            }
            return imageUrls;
        }
    }
}
