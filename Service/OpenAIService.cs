using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Images;

namespace OpenAI_POC.Service
{
    public class OpenAIService : IOpenAIService
    {
        private readonly string _apiKey;
        private readonly EmbeddingClient _embeddingClient;

        public OpenAIService(IConfiguration config, EmbeddingClient embeddingClient)
        {
            _embeddingClient = embeddingClient ?? throw new ArgumentNullException(nameof(embeddingClient));
            _apiKey = config["OpenAI:openai_key"]
                      ?? throw new InvalidOperationException("OpenAI API key is not configured");
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
        public ReadOnlyMemory<float> GetEmbeddingVector(string input)
        {
            var result = _embeddingClient.GenerateEmbedding(input);
            return result.Value.ToFloats();
        }
    }
}
