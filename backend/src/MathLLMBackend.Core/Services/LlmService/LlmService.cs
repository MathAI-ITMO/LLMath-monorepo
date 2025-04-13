using System.ClientModel;
using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Core.Services.PromptService;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace MathLLMBackend.Core.Services.LlmService;

public class LlmService : ILlmService
{
    private readonly IOptions<LlmServiceConfiguration> _config;
    private readonly IPromptService _promptService;

    public LlmService(IOptions<LlmServiceConfiguration> config, IPromptService promptService)
    {
        _config = config;
        _promptService = promptService;
    }
    
    public async IAsyncEnumerable<string> GenerateNextMessageStreaming(List<Message> messages, CancellationToken ct)
    {
        var config = _config.Value.ChatModel;

        var client = new ChatClient(
            model: config.Model,
            credential: new ApiKeyCredential(config.Token),
            options: new OpenAIClientOptions() { Endpoint = new Uri(config.Url) });
        

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
            ct.ThrowIfCancellationRequested();
            if (chunk.ContentUpdate.Count == 0)
                break;
            
            yield return chunk.ContentUpdate[0].Text;
        }
    }

    public async Task<string> SolveProblem(string message, CancellationToken ct)
    {
        var config = _config.Value.SolverModel;

        var client = new ChatClient(
            model: config.Model,
            credential: new ApiKeyCredential(config.Token),
            options: new OpenAIClientOptions() { Endpoint = new Uri(config.Url) });
        

        var openaiMessages = new ChatMessage[]
            {
                new SystemChatMessage(_promptService.GetSolverSystemPrompt()),
                new UserChatMessage(_promptService.GetSolverTaskPrompt(message)),
            };
    
        var completion = await client.CompleteChatAsync(openaiMessages, cancellationToken: ct);
        
        return completion!.Value.Content[0].Text;
    }

    public async Task<string> GenerateNextMessageAsync(List<Message> messages, CancellationToken ct)
    {
        var config = _config.Value.ChatModel;

        var client = new ChatClient(
            model: config.Model,
            credential: new ApiKeyCredential(config.Token),
            options: new OpenAIClientOptions() { Endpoint = new Uri(config.Url) });
        

        var openaiMessages = messages.Select<Message, ChatMessage>(m =>
            m.MessageType switch
            {
                MessageType.User => new UserChatMessage(m.Text),
                MessageType.Assistant => new AssistantChatMessage(m.Text),
                MessageType.System => new SystemChatMessage(m.Text),
                _ => throw new NotImplementedException()
            }
        );
    
        var completion = await client.CompleteChatAsync(openaiMessages, cancellationToken: ct);
        
        return completion!.Value.Content[0].Text;
    }
}