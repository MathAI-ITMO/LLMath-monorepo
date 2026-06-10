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
    private readonly ILogger<LlmService> _logger;

    public LlmService(
        IOptions<LlmServiceConfiguration> config,
        IPromptService promptService,
        ILogger<LlmService> logger)
    {
        _config = config;
        _promptService = promptService;
        _logger = logger;
    }

    public async Task<string> SolveProblem(string problemDescription, CancellationToken ct)
    {
        var config = _config.Value.SolverModel;
        var client = CreateChatClient(config);

        var solverSystemPrompt = _promptService.GetSolverSystemPrompt();
        var solverTaskPrompt = _promptService.GetSolverTaskPrompt(problemDescription);

        var openaiMessages = new ChatMessage[]
            {
                new SystemChatMessage(solverSystemPrompt),
                new UserChatMessage(solverTaskPrompt),
            };

        var completion = await client.CompleteChatAsync(openaiMessages, cancellationToken: ct);
        var content = completion?.Value.Content;
        if (content == null || content.Count == 0)
            throw new InvalidOperationException("LLM returned empty content");
        var solution = content[0].Text;

        _logger.LogLlmSolution(problemDescription, solution, config.Model);

        return solution;
    }

    public async Task<string> GenerateNextMessageAsync(List<Message> messages, TaskType taskType, CancellationToken ct)
    {
        var client = CreateChatClient(_config.Value.ChatModel);

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
        var content = completion?.Value.Content;
        if (content == null || content.Count == 0)
            throw new InvalidOperationException("LLM returned empty content");
        var response = content[0].Text;
        var config = _config.Value.ChatModel;

        _logger.LogLlmInteraction(taskType, messages, response, config.Model);

        return response;
    }

    public async Task<string> ExtractAnswer(string problemStatement, string solution, CancellationToken ct)
    {
        var client = CreateChatClient(_config.Value.SolverModel);

        var extractAnswerSystemPrompt = _promptService.GetExtractAnswerSystemPrompt();
        var extractAnswerPrompt = _promptService.GetExtractAnswerPrompt(problemStatement, solution);

        var openaiMessages = new ChatMessage[]
            {
                new SystemChatMessage(extractAnswerSystemPrompt),
                new UserChatMessage(extractAnswerPrompt),
            };

        var completion = await client.CompleteChatAsync(openaiMessages, cancellationToken: ct);
        var content = completion?.Value.Content;
        if (content == null || content.Count == 0)
            throw new InvalidOperationException("LLM returned empty content");
        var extractedAnswer = content[0].Text;

        _logger.LogInformation("Extracted answer: {ExtractedAnswer}", extractedAnswer);

        return extractedAnswer;
    }

    private static ChatClient CreateChatClient(ModelConfiguration config)
    {
        return new ChatClient(
            model: config.Model,
            credential: new ApiKeyCredential(config.Token),
            options: new OpenAIClientOptions { Endpoint = new Uri(config.Url) });
    }
}