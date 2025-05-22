using System.ClientModel;
using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Core.Services.PromptService;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.Runtime.CompilerServices;
using System.Text;

namespace MathLLMBackend.Core.Services.LlmService;

public class LlmService : ILlmService
{
    private readonly IOptions<LlmServiceConfiguration> _config;
    private readonly IPromptService _promptService;
    private readonly ILlmLoggingService _loggingService;
    private readonly ILogger<LlmService> _logger;

    public LlmService(
        IOptions<LlmServiceConfiguration> config, 
        IPromptService promptService,
        ILlmLoggingService loggingService,
        ILogger<LlmService> logger)
    {
        _config = config;
        _promptService = promptService;
        _loggingService = loggingService;
        _logger = logger;
    }
    
    public async IAsyncEnumerable<string> GenerateNextMessageStreaming(List<Message> messages, int taskType, [EnumeratorCancellation] CancellationToken ct)
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
    
        var fullResponseText = new StringBuilder();
        AsyncCollectionResult<StreamingChatCompletionUpdate> completion = client.CompleteChatStreamingAsync(openaiMessages, cancellationToken: ct);

        await foreach (var chunk in completion)
        {
            ct.ThrowIfCancellationRequested();
            if (chunk.ContentUpdate.Count > 0 && !string.IsNullOrEmpty(chunk.ContentUpdate[0].Text))
            {
                var textChunk = chunk.ContentUpdate[0].Text;
                fullResponseText.Append(textChunk);
                yield return textChunk;
        }
    }
        // Log the full interaction after streaming is complete
        await _loggingService.LogInteraction(taskType, messages, fullResponseText.ToString(), config.Model);
    }

    public async Task<string> SolveProblem(string problemDescription, CancellationToken ct)
    {
        var config = _config.Value.SolverModel;

        var client = new ChatClient(
            model: config.Model,
            credential: new ApiKeyCredential(config.Token),
            options: new OpenAIClientOptions() { Endpoint = new Uri(config.Url) });
        
        var solverSystemPrompt = _promptService.GetSolverSystemPrompt();
        var solverTaskPrompt = _promptService.GetSolverTaskPrompt(problemDescription);

        var openaiMessages = new ChatMessage[]
            {
                new SystemChatMessage(solverSystemPrompt),
                new UserChatMessage(solverTaskPrompt),
            };
    
        var completion = await client.CompleteChatAsync(openaiMessages, cancellationToken: ct);
        var solution = completion!.Value.Content[0].Text;
        
        await _loggingService.LogSolution(problemDescription, solution, config.Model);
        
        return solution;
    }

    public async Task<string> GenerateNextMessageAsync(List<Message> messages, int taskType, CancellationToken ct)
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
        var response = completion!.Value.Content[0].Text;
        
        await _loggingService.LogInteraction(taskType, messages, response, config.Model);
        
        return response;
    }
}