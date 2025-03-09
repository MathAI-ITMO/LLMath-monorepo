using System.ClientModel;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using OpenAI;
using OpenAI.Chat;

namespace MathLLMBackend.Core.Services.LlmService;

public class LlmService : ILlmService
{
    public async IAsyncEnumerable<string> GenerateNextMessageText(List<Message> messages)
    {
        var client = new ChatClient (
            model: "gpt-3.5-turbo",
            credential: new ApiKeyCredential("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6ImM3Y2U4MGI0LTMwZDEtNGFhZi05NTZlLTNmMWU3N2Y1M2RjOSIsImlzRGV2ZWxvcGVyIjp0cnVlLCJpYXQiOjE3NDExMjI3NzgsImV4cCI6MjA1NjY5ODc3OH0.jHI9lFuVsRX237_6iyBmVwHFrzI8_ar7t3yUYdwjuxo"),
            options: new OpenAIClientOptions() { Endpoint =  new Uri("https://bothub.chat/api/v2/openai/v1")} );
    
        var openaiMessages = messages.Select<Message, ChatMessage>(m =>
            m.MessageType switch
            {
                MessageType.User => new UserChatMessage(m.Text),
                MessageType.Assistant => new AssistantChatMessage(m.Text),
                MessageType.System => new SystemChatMessage(m.Text),
                _ => throw new NotImplementedException()
            }
        );
    
        AsyncCollectionResult<StreamingChatCompletionUpdate> completion = client.CompleteChatStreamingAsync(openaiMessages);

        await foreach (var chunk in completion)
        {
            yield return chunk.ContentUpdate[0].Text;
        }
    }
}