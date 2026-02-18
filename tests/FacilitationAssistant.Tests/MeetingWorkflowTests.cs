using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FacilitationAssistant.Tests;

public class MeetingWorkflowTests
{
    private FacilitationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<FacilitationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FacilitationDbContext(options);
    }

    [Fact]
    public async Task CreateMeeting_ShouldGenerateUniqueTokens()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var handler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
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
        var context = CreateInMemoryContext();
        var createHandler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(context);

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
        
        var savedMeeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == meeting.MeetingId);
        
        Assert.NotNull(savedMeeting);
        
        var savedStage = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == stageId);
        
        Assert.NotNull(savedStage);
        Assert.Equal("Introduction", savedStage.Name);
        Assert.Equal(10, savedStage.PlannedDurationMinutes);
    }

    [Fact]
    public async Task StartMeeting_ShouldChangeStatusToActive()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var createHandler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
        var startHandler = new StartMeetingHandler(context);

        var createCommand = new CreateMeetingCommand("Test Meeting");
        var meeting = await createHandler.Handle(createCommand, CancellationToken.None);

        var startCommand = new StartMeetingCommand(meeting.MeetingId);

        // Act
        await startHandler.Handle(startCommand, CancellationToken.None);

        // Assert
        var savedMeeting = await context.Meetings.FindAsync(meeting.MeetingId);
        Assert.NotNull(savedMeeting);
        Assert.Equal(MeetingStatus.Active, savedMeeting.Status);
        Assert.NotNull(savedMeeting.StartedAt);
    }

    [Fact]
    public async Task StartStage_ShouldActivateStageAndEndPrevious()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var createHandler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(context);
        var startStageHandler = new StartStageHandler(context);

        var meeting = await createHandler.Handle(new CreateMeetingCommand("Test"), CancellationToken.None);
        
        var stage1Id = await addStageHandler.Handle(
            new AddAgendaStageCommand(meeting.MeetingId, "Stage 1", null, 10, 0),
            CancellationToken.None);
        
        var stage2Id = await addStageHandler.Handle(
            new AddAgendaStageCommand(meeting.MeetingId, "Stage 2", null, 15, 1),
            CancellationToken.None);

        // Act - Start first stage
        await startStageHandler.Handle(new StartStageCommand(meeting.MeetingId, stage1Id), CancellationToken.None);
        
        var afterFirstStart = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == stage1Id);
        
        Assert.NotNull(afterFirstStart);
        Assert.Equal(StageStatus.Active, afterFirstStart.Status);

        // Wait a moment to ensure time difference
        await Task.Delay(100);

        // Start second stage
        await startStageHandler.Handle(new StartStageCommand(meeting.MeetingId, stage2Id), CancellationToken.None);

        // Assert
        var stage1 = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == stage1Id);
        var stage2 = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == stage2Id);

        Assert.NotNull(stage1);
        Assert.NotNull(stage2);
        Assert.Equal(StageStatus.Completed, stage1.Status);
        Assert.Equal(StageStatus.Active, stage2.Status);
        Assert.NotNull(stage1.CompletedAt);
        Assert.NotNull(stage2.StartedAt);
    }

    [Fact]
    public async Task GetMeetingByToken_ShouldReturnMeetingWithStages()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var createHandler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(context);
        var getMeetingHandler = new GetMeetingByTokenHandler(context, NullLogger<GetMeetingByTokenHandler>.Instance);

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
        
        var stages = await context.AgendaStages
            .Where(s => s.MeetingId == result.Id)
            .ToListAsync();
        
        Assert.Single(stages);
        Assert.Equal("Stage 1", stages[0].Name);
    }

    [Fact]
    public async Task AddNote_ShouldCreateNoteForMeeting()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var createHandler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
        var addNoteHandler = new AddNoteHandler(context);

        var meeting = await createHandler.Handle(new CreateMeetingCommand("Test"), CancellationToken.None);
        var command = new AddNoteCommand(meeting.MeetingId, "session-1", "Test note content", true);

        // Act
        var noteId = await addNoteHandler.Handle(command, CancellationToken.None);

        // Assert
        var savedNote = await context.Notes
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
        var context = CreateInMemoryContext();
        var createHandler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
        var raiseConcernHandler = new RaiseConcernHandler(context);

        var meeting = await createHandler.Handle(new CreateMeetingCommand("Test"), CancellationToken.None);
        var command = new RaiseConcernCommand(meeting.MeetingId, "session-1", "Too Fast", "Moving too quickly");

        // Act
        var concernId = await raiseConcernHandler.Handle(command, CancellationToken.None);

        // Assert
        var savedConcern = await context.Concerns
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
        var context = CreateInMemoryContext();
        var createHandler = new CreateMeetingHandler(context, NullLogger<CreateMeetingHandler>.Instance);
        var addStageHandler = new AddAgendaStageHandler(context);
        var startMeetingHandler = new StartMeetingHandler(context);
        var startStageHandler = new StartStageHandler(context);
        var endStageHandler = new EndStageHandler(context);

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
        var finalMeeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == meeting.MeetingId);

        Assert.NotNull(finalMeeting);
        Assert.Equal(MeetingStatus.Active, finalMeeting.Status);
        
        var stages = await context.AgendaStages
            .Where(s => s.MeetingId == meeting.MeetingId)
            .ToListAsync();
        
        Assert.Equal(2, stages.Count);
        
        var completedStage = stages.First(s => s.Id == stage1);
        Assert.Equal(StageStatus.Completed, completedStage.Status);
        Assert.NotNull(completedStage.ActualDurationSeconds);
    }
}
