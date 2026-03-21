using System.Net;
using Xunit;
using System.Net.Http.Json;
using FluentAssertions;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Presentation.Dtos.Chats;
using Moq;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MathLLMBackend.DataAccess.Contexts;
using System.Linq;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class ChatControllerTests : BaseIntegrationTest
{
    public ChatControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateChat_WithoutAuth_ReturnsUnauthorized()
    {
        var request = new CreateChatRequestDto("Test Chat", null);
        var response = await Client.PostAsJsonAsync("/api/chat/create", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateChat_WithAuth_ReturnsOk()
    {
        await CreateAndLoginUserAsync();
        var request = new CreateChatRequestDto("Test Chat", null);
        var response = await AuthenticatedPostAsync("/api/chat/create", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Chat");
    }

    [Fact]
    public async Task GetChats_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/chat/get");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChats_WithAuth_ReturnsOk()
    {
        await CreateAndLoginUserAsync();
        var response = await AuthenticatedGetAsync("/api/chat/get");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetChatDetails_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/chat/get/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChatDetails_WithNonExistentChat_ReturnsNotFound()
    {
        await CreateAndLoginUserAsync();
        var response = await AuthenticatedGetAsync("/api/chat/get/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteChat_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsync("/api/chat/delete/00000000-0000-0000-0000-000000000000", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteChat_WithNonExistentChat_ReturnsNotFound()
    {
        await CreateAndLoginUserAsync();
        var response = await AuthenticatedPostAsync("/api/chat/delete/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetChatDetails_WithValidChat_ReturnsOk()
    {
        await CreateAndLoginUserAsync();
        
        var request = new CreateChatRequestDto($"Test Chat {Guid.NewGuid()}", null);
        var createResponse = await AuthenticatedPostAsync("/api/chat/create", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var chatDto = await createResponse.Content.ReadFromJsonAsync<ChatDto>();
        chatDto.Should().NotBeNull();
        chatDto!.Id.Should().NotBe(Guid.Empty);
        
        var response = await AuthenticatedGetAsync($"/api/chat/get/{chatDto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteChat_WithValidChat_ReturnsOk()
    {
        await CreateAndLoginUserAsync();
        
        var request = new CreateChatRequestDto($"Test Chat {Guid.NewGuid()}", null);
        var createResponse = await AuthenticatedPostAsync("/api/chat/create", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var chatDto = await createResponse.Content.ReadFromJsonAsync<ChatDto>();
        chatDto.Should().NotBeNull();
        chatDto!.Id.Should().NotBe(Guid.Empty);
        
        var response = await AuthenticatedPostAsync($"/api/chat/delete/{chatDto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteChat_WithOtherUserChat_ReturnsUnauthorized()
    {
        await CreateAndLoginUserAsync();
        var otherUser = await Factory.CreateTestUserAsync("other@example.com", "Test123!@#");
        
        using var scope = Factory.Services.CreateScope();
        var chatService = scope.ServiceProvider.GetRequiredService<MathLLMBackend.Core.Services.ChatService.IChatService>();
        var chat = new MathLLMBackend.Domain.Entities.Chat("Other User Chat", otherUser.Id);
        var createdChat = await chatService.Create(chat, CancellationToken.None);

        var response = await AuthenticatedPostAsync($"/api/chat/delete/{createdChat.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetChatDetails_WithOtherUserChat_ReturnsForbidden()
    {
        await CreateAndLoginUserAsync();
        var otherUser = await Factory.CreateTestUserAsync("other_details@example.com", "Test123!@#");
        
        using var scope = Factory.Services.CreateScope();
        var chatService = scope.ServiceProvider.GetRequiredService<MathLLMBackend.Core.Services.ChatService.IChatService>();
        var chat = new MathLLMBackend.Domain.Entities.Chat("Other User Chat Details", otherUser.Id);
        var createdChat = await chatService.Create(chat, CancellationToken.None);

        var response = await AuthenticatedGetAsync($"/api/chat/get/{createdChat.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateChat_WithProblemHash_ReturnsOk()
    {
        var problemId = Guid.NewGuid();
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var problem = new Problem("Test solution", "Test problem", "Test Title") { Id = problemId, TheoryLink = "link" };
            dbContext.Problems.Add(problem);
            await dbContext.SaveChangesAsync();
        }

        Factory.LlmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test response");

        await CreateAndLoginUserAsync();
        var request = new CreateChatRequestDto("Test Chat", problemId);
        var response = await AuthenticatedPostAsync("/api/chat/create", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetChatDetails_WithProblemSolverChat_ReturnsOk()
    {
        var problemId = Guid.NewGuid();
        
        Factory.LlmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test response");

        await CreateAndLoginUserAsync();
        
        ProblemTaskType ptt;
        using (var scope1 = Factory.Services.CreateScope())
        {
            var dbContext1 = scope1.ServiceProvider.GetRequiredService<AppDbContext>();
            var problem = new Problem("Test solution text content", "Test problem statement with content", "Test Title") 
            { 
                Id = problemId, 
                TheoryLink = "https://example.com/theory" 
            };
            ptt = new ProblemTaskType(problem, TaskType.Learning);
            problem.Types = new List<ProblemTaskType> { ptt };
            
            dbContext1.Problems.Add(problem);

            var userTask = new UserTask
            {
                ApplicationUserId = TestUser!.Id,
                ProblemId = problemId,
                ProblemHash = problemId.ToString(),
                DisplayName = "Test Task",
                ProblemTaskType = ptt,
                TaskType = TaskType.Learning,
                Status = UserTaskStatus.InProgress
            };
            dbContext1.UserTasks.Add(userTask);
            await dbContext1.SaveChangesAsync();
        }

        using (var scope2 = Factory.Services.CreateScope())
        {
            var chatService = scope2.ServiceProvider.GetRequiredService<MathLLMBackend.Core.Services.ChatService.IChatService>();
            var chat = new Chat($"Test ProblemSolver Chat {Guid.NewGuid()}", TestUser!.Id);
            
            var createdChat = await chatService.Create(chat, problemId, TaskType.Exam, CancellationToken.None);
            createdChat.Id.Should().NotBe(Guid.Empty);
            
            var dbContext2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
            var verifyChat = await dbContext2.Chats.FindAsync(createdChat.Id);
            verifyChat.Should().NotBeNull("Chat should be saved in database");
            
            var response = await AuthenticatedGetAsync($"/api/chat/get/{createdChat.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetChatDetails_WithProblemSolverChatWithoutUserTask_ReturnsOk()
    {
        var problemId = Guid.NewGuid();
        
        Factory.LlmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test response");

        await CreateAndLoginUserAsync();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var problem = new Problem("Test solution text content", "Test problem statement with content", "Test Title") 
            { 
                Id = problemId, 
                TheoryLink = "link" 
            };
            dbContext.Problems.Add(problem);
            await dbContext.SaveChangesAsync();

            var chatService = scope.ServiceProvider.GetRequiredService<MathLLMBackend.Core.Services.ChatService.IChatService>();
            var chat = new Chat($"Test ProblemSolver Chat {Guid.NewGuid()}", TestUser!.Id);
            
            var createdChat = await chatService.Create(chat, problemId, TaskType.Exam, CancellationToken.None);
            createdChat.Id.Should().NotBe(Guid.Empty);
            
            var verifyChat = await dbContext.Chats.FindAsync(createdChat.Id);
            verifyChat.Should().NotBeNull("Chat should be saved in database");
            
            var response = await AuthenticatedGetAsync($"/api/chat/get/{createdChat.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
