using System.ClientModel;
using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace MathLLMBackend.Core.Services.LlmService;

public class LlmService : ILlmService
{
    private readonly IOptions<LlmServiceConfiguration> _config;

    public LlmService(IOptions<LlmServiceConfiguration> config)
    {
        _config = config;
    }
    
    public async IAsyncEnumerable<string> GenerateNextMessageText(List<Message> messages)
    {
        var client = new ChatClient (
            model: _config.Value.Model,
            credential: new ApiKeyCredential(_config.Value.Token),
            options: new OpenAIClientOptions() { Endpoint =  new Uri(_config.Value.Url)} );
    
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
            if (chunk.ContentUpdate.Count == 0)
                break;
            
            yield return chunk.ContentUpdate[0].Text;
        }
    }
}