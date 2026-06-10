using System.Net;
using Xunit;
using System.Net.Http.Json;
using FluentAssertions;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Presentation.Dtos.Messages;
using MathLLMBackend.Presentation.Dtos.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;
using MathLLMBackend.DataAccess.Contexts;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class MessageControllerTests : BaseIntegrationTest
{
    public MessageControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Complete_WithoutAuth_ReturnsUnauthorized()
    {
        var request = new MessageCreateDto(Guid.NewGuid(), "Test message");
        var response = await Client.PostAsJsonAsync("/api/message/complete", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Complete_WithValidChat_ReturnsOk()
    {
        await CreateAndLoginUserAsync();

        Factory.LlmServiceMock.Reset();
        Factory.LlmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test response");

        var createRequest = new CreateChatRequestDto($"Test Chat {Guid.NewGuid()}", null);
        var createResponse = await AuthenticatedPostAsync("/api/chat/create", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var chatDto = await createResponse.Content.ReadFromJsonAsync<ChatDto>();
        chatDto.Should().NotBeNull();
        chatDto!.Id.Should().NotBe(Guid.Empty);

        using (var verifyScope = Factory.Services.CreateScope())
        {
            var dbContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var verifyChat = await dbContext.Chats.FindAsync(chatDto.Id);
            verifyChat.Should().NotBeNull("Chat should be visible in database");
            verifyChat!.UserId.Should().Be(TestUser!.Id);
        }

        var request = new MessageCreateDto(chatDto.Id, "Test message");
        var response = await AuthenticatedPostAsync("/api/message/complete", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllMessagesFromChat_WithValidChat_ReturnsOk()
    {
        await CreateAndLoginUserAsync();

        using var scope = Factory.Services.CreateScope();
        var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
        var chat = new Chat("Test Chat", TestUser!.Id);
        var createdChat = await chatService.Create(chat, CancellationToken.None);

        var response = await AuthenticatedGetAsync($"/api/message/get-messages-from-chat?chatId={createdChat.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllMessagesFromChat_WithNonExistentChat_ReturnsNotFound()
    {
        await CreateAndLoginUserAsync();

        var response = await AuthenticatedGetAsync("/api/message/get-messages-from-chat?chatId=00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Complete_WithOtherUserChat_ReturnsForbid()
    {
        await CreateAndLoginUserAsync();
        var otherUser = await Factory.CreateTestUserAsync("other@example.com", "Test123!@#");

        using var scope = Factory.Services.CreateScope();
        var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
        var chat = new Chat("Other User Chat", otherUser.Id);
        var createdChat = await chatService.Create(chat, CancellationToken.None);

        var request = new MessageCreateDto(createdChat.Id, "Test message");
        var response = await AuthenticatedPostAsync("/api/message/complete", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Complete_WithEmptyLlmResponse_ReturnsOk()
    {
        await CreateAndLoginUserAsync();

        Factory.LlmServiceMock.Reset();
        Factory.LlmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        var createRequest = new CreateChatRequestDto($"Test Chat {Guid.NewGuid()}", null);
        var createResponse = await AuthenticatedPostAsync("/api/chat/create", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var chatDto = await createResponse.Content.ReadFromJsonAsync<ChatDto>();
        chatDto.Should().NotBeNull();
        chatDto!.Id.Should().NotBe(Guid.Empty);

        using (var verifyScope = Factory.Services.CreateScope())
        {
            var dbContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var verifyChat = await dbContext.Chats.FindAsync(chatDto.Id);
            verifyChat.Should().NotBeNull("Chat should be visible in database");
            verifyChat!.UserId.Should().Be(TestUser!.Id);
        }

        var request = new MessageCreateDto(chatDto.Id, "Test message");
        var response = await AuthenticatedPostAsync("/api/message/complete", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEmpty();
    }
}
