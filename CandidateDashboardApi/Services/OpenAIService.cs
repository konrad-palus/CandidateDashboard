using CandidateDashboardApi.Interfaces;
using OpenAI_API;
using OpenAI_API.Chat;

namespace CandidateDashboardApi.Services
{


    public class OpenAIService : IOpenAIService
    {
        private readonly OpenAIAPI _openAiClient;

        public OpenAIService(string apiKey)
        {
            _openAiClient = new OpenAIAPI(apiKey); 
        }

        public async Task<ChatMessage> GetChatResponseAsync(string userMessage)
        {
            try
            {
                var response = await _openAiClient.Chat.CreateChatCompletionAsync(
                    model: "gpt-3.5-turbo",
                    messages: new List<ChatMessage>
                    {
                        new ChatMessage
                        {
                            Role = ChatMessageRole.User,
                            Content = userMessage
                        }
                    },
                    max_tokens: 80
                );

                return (response != null && response.Choices != null && response.Choices.Count > 0)
                        ? response.Choices.Last().Message
                        : new ChatMessage { Role = ChatMessageRole.System, Content = "No response received." };
            }
            catch (Exception ex)
            {
                return new ChatMessage { Role = ChatMessageRole.System, Content = $"An error occurred while trying to get a response: {ex}" };
            }
        }
    }
}
