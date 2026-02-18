using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Handlers;
using FacilitationAssistant.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FacilitationAssistant.Tests;

public class MeetingWorkflowTests
{
    private DbContextOptions<FacilitationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<FacilitationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private IDbContextFactory<FacilitationDbContext> CreateDbContextFactory(DbContextOptions<FacilitationDbContext> options)
    {
        var mockFactory = new Mock<IDbContextFactory<FacilitationDbContext>>();
        mockFactory
            .Setup(f => f.CreateDbContext())
            .Returns(() => new FacilitationDbContext(options));
        mockFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken _) => new FacilitationDbContext(options));
        
        return mockFactory.Object;
    }

    private FacilitationDbContext CreateContext(DbContextOptions<FacilitationDbContext> options)
    {
        return new FacilitationDbContext(options);
    }

    private IHubContext<MeetingHub> CreateMockHubContext()
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        
        var mockHubContext = new Mock<IHubContext<MeetingHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        
        return mockHubContext.Object;
    }

    [Fact]
    public async Task CreateMeeting_ShouldGenerateUniqueTokens()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var handler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var command = new CreateMeetingCommand("Test Meeting");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.MeetingId);
        Assert.NotEmpty(result.FacilitatorToken);
        Assert.NotEmpty(result.AttendeeToken);
        Assert.NotEqual(result.FacilitatorToken, result.AttendeeToken);
    }

    [Fact]
    public async Task AddAgendaStage_ShouldAddStageToMeeting()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var createHandler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(factory);

        var createCommand = new CreateMeetingCommand("Test Meeting");
        var meeting = await createHandler.Handle(createCommand, CancellationToken.None);

        var stageCommand = new AddAgendaStageCommand(
            meeting.MeetingId,
            "Introduction",
            "Welcome and introductions",
            10,
            0
        );

        // Act
        var stageId = await addStageHandler.Handle(stageCommand, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, stageId);
        
        using var context = CreateContext(options);
        var savedMeeting = await context.Meetings
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == meeting.MeetingId);
        
        Assert.NotNull(savedMeeting);
        
        var savedStage = await context.AgendaStages
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == stageId);
        
        Assert.NotNull(savedStage);
        Assert.Equal("Introduction", savedStage.Name);
        Assert.Equal(10, savedStage.PlannedDurationMinutes);
    }

    [Fact]
    public async Task StartMeeting_ShouldChangeStatusToActive()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var hubContext = CreateMockHubContext();
        var createHandler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var startHandler = new StartMeetingHandler(factory, hubContext);

        var createCommand = new CreateMeetingCommand("Test Meeting");
        var meeting = await createHandler.Handle(createCommand, CancellationToken.None);

        var startCommand = new StartMeetingCommand(meeting.MeetingId);

        // Act
        await startHandler.Handle(startCommand, CancellationToken.None);

        // Assert
        using var context = CreateContext(options);
        var savedMeeting = await context.Meetings
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == meeting.MeetingId);
        Assert.NotNull(savedMeeting);
        Assert.Equal(MeetingStatus.Active, savedMeeting.Status);
        Assert.NotNull(savedMeeting.StartedAt);
    }

    [Fact]
    public async Task StartStage_ShouldActivateStageAndEndPrevious()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var hubContext = CreateMockHubContext();
        var createHandler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(factory);
        var startStageHandler = new StartStageHandler(factory, hubContext);

        var meeting = await createHandler.Handle(new CreateMeetingCommand("Test"), CancellationToken.None);
        
        var stage1Id = await addStageHandler.Handle(
            new AddAgendaStageCommand(meeting.MeetingId, "Stage 1", null, 10, 0),
            CancellationToken.None);
        
        var stage2Id = await addStageHandler.Handle(
            new AddAgendaStageCommand(meeting.MeetingId, "Stage 2", null, 15, 1),
            CancellationToken.None);

        // Act - Start first stage
        await startStageHandler.Handle(new StartStageCommand(meeting.MeetingId, stage1Id), CancellationToken.None);
        
        // Verify first stage is active (use new context to avoid caching)
        using (var context = CreateContext(options))
        {
            var afterFirstStart = await context.AgendaStages
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stage1Id);
            
            Assert.NotNull(afterFirstStart);
            Assert.Equal(StageStatus.Active, afterFirstStart.Status);
        }

        // Wait a moment to ensure time difference
        await Task.Delay(100);

        // Start second stage
        await startStageHandler.Handle(new StartStageCommand(meeting.MeetingId, stage2Id), CancellationToken.None);

        // Assert - Use new context to get fresh data
        using (var context = CreateContext(options))
        {
            var stage1 = await context.AgendaStages
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stage1Id);
            var stage2 = await context.AgendaStages
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stage2Id);

            Assert.NotNull(stage1);
            Assert.NotNull(stage2);
            Assert.Equal(StageStatus.Completed, stage1.Status);
            Assert.Equal(StageStatus.Active, stage2.Status);
            Assert.NotNull(stage1.CompletedAt);
            Assert.NotNull(stage2.StartedAt);
        }
    }

    [Fact]
    public async Task GetMeetingByToken_ShouldReturnMeetingWithStages()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var createHandler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(factory);
        var getMeetingHandler = new GetMeetingByTokenHandler(factory, NullLogger<GetMeetingByTokenHandler>.Instance);

        var meeting = await createHandler.Handle(new CreateMeetingCommand("Test"), CancellationToken.None);
        await addStageHandler.Handle(
            new AddAgendaStageCommand(meeting.MeetingId, "Stage 1", null, 10, 0),
            CancellationToken.None);

        // Act
        var query = new GetMeetingByTokenQuery(meeting.FacilitatorToken, true);
        var result = await getMeetingHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(meeting.MeetingId, result.Id);
        
        using var context = CreateContext(options);
        var stages = await context.AgendaStages
            .AsNoTracking()
            .Where(s => s.MeetingId == result.Id)
            .ToListAsync();
        
        Assert.Single(stages);
        Assert.Equal("Stage 1", stages[0].Name);
    }

    [Fact]
    public async Task AddNote_ShouldCreateNoteForMeeting()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var hubContext = CreateMockHubContext();
        var createHandler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var addNoteHandler = new AddNoteHandler(factory, hubContext);

        var meeting = await createHandler.Handle(new CreateMeetingCommand("Test"), CancellationToken.None);
        var command = new AddNoteCommand(meeting.MeetingId, "session-1", "Test note content", true);

        // Act
        var noteId = await addNoteHandler.Handle(command, CancellationToken.None);

        // Assert
        using var context = CreateContext(options);
        var savedNote = await context.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == noteId);
        
        Assert.NotNull(savedNote);
        Assert.Equal("Test note content", savedNote.Content);
        Assert.True(savedNote.IsPublic);
        Assert.Equal("session-1", savedNote.SessionId);
    }

    [Fact]
    public async Task RaiseConcern_ShouldCreateConcernForMeeting()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var hubContext = CreateMockHubContext();
        var createHandler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var raiseConcernHandler = new RaiseConcernHandler(factory, hubContext);

        var meeting = await createHandler.Handle(new CreateMeetingCommand("Test"), CancellationToken.None);
        var command = new RaiseConcernCommand(meeting.MeetingId, "session-1", "Too Fast", "Moving too quickly");

        // Act
        var concernId = await raiseConcernHandler.Handle(command, CancellationToken.None);

        // Assert
        using var context = CreateContext(options);
        var savedConcern = await context.Concerns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == concernId);
        
        Assert.NotNull(savedConcern);
        Assert.Equal("Too Fast", savedConcern.ConcernType);
        Assert.Equal("Moving too quickly", savedConcern.CustomText);
        Assert.False(savedConcern.IsDismissed);
    }

    [Fact]
    public async Task CompleteWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var factory = CreateDbContextFactory(options);
        var hubContext = CreateMockHubContext();
        var createHandler = new CreateMeetingHandler(factory, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(factory);
        var startMeetingHandler = new StartMeetingHandler(factory, hubContext);
        var startStageHandler = new StartStageHandler(factory, hubContext);
        var endStageHandler = new EndStageHandler(factory, hubContext);

        // Act - Create meeting
        var meeting = await createHandler.Handle(new CreateMeetingCommand("Full Workflow Test"), CancellationToken.None);

        // Add stages
        var stage1 = await addStageHandler.Handle(
            new AddAgendaStageCommand(meeting.MeetingId, "Introduction", null, 5, 0),
            CancellationToken.None);
        var stage2 = await addStageHandler.Handle(
            new AddAgendaStageCommand(meeting.MeetingId, "Discussion", null, 10, 1),
            CancellationToken.None);

        // Start meeting
        await startMeetingHandler.Handle(new StartMeetingCommand(meeting.MeetingId), CancellationToken.None);

        // Start first stage
        await startStageHandler.Handle(new StartStageCommand(meeting.MeetingId, stage1), CancellationToken.None);

        // End first stage
        await Task.Delay(100);
        await endStageHandler.Handle(new EndStageCommand(meeting.MeetingId, stage1), CancellationToken.None);

        // Assert
        using var context = CreateContext(options);
        var finalMeeting = await context.Meetings
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == meeting.MeetingId);

        Assert.NotNull(finalMeeting);
        Assert.Equal(MeetingStatus.Active, finalMeeting.Status);
        
        var stages = await context.AgendaStages
            .AsNoTracking()
            .Where(s => s.MeetingId == meeting.MeetingId)
            .ToListAsync();
        
        Assert.Equal(2, stages.Count);
        
        var completedStage = stages.First(s => s.Id == stage1);
        Assert.Equal(StageStatus.Completed, completedStage.Status);
        Assert.NotNull(completedStage.ActualDurationSeconds);
    }
}
