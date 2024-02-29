using OpenAI_API.Chat;

namespace CandidateDashboardApi.Interfaces
{
    public interface IOpenAIService
    {
        Task<ChatMessage> GetChatResponseAsync(string userMessage);
    }
}
