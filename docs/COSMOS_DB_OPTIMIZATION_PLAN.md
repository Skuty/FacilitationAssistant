# Cosmos DB Optimization Plan - Separate Documents Approach

## Executive Summary

Revert from owned entities to separate documents to solve concurrency issues while optimizing for Azure Cosmos DB Free Tier (1000 RU/s).

**Status:** Planning Phase  
**Priority:** High - Fixes critical concurrency conflicts  
**Effort:** Medium (2-3 days)

---

## Problem Statement

### Current Issues with Owned Entities
1. **Concurrency conflicts**: Multiple attendees modifying same Meeting document simultaneously causes 412 errors
2. **2MB document limit**: Large meetings with many responses risk exceeding Cosmos DB document size limit
3. **All-or-nothing updates**: Any child entity change locks entire Meeting document

### Example Failure Scenario
```
Meeting with 20 attendees:
- 10:00: Attendee A adds note → ETag v100 → v101 ✅
- 10:00: Attendee B raises concern → ETag v100 → CONFLICT ❌
- 10:00: Attendee C submits response → ETag v100 → CONFLICT ❌
```
**Result:** 66% failure rate on writes!

---

## Proposed Solution: Separate Documents (No Partitioning)

### Architecture

**Each entity type = separate Cosmos DB document**

```
Containers (in single database):
├── Meetings (root documents)
├── AgendaStages 
├── Notes
├── Concerns
├── ConcernVotes
├── AttendeeSessions
├── Questions
├── QuestionOptions
├── QuestionResponses
├── Messages
├── MessageOptions
└── MessageResponses
```

**Benefits:**
- ✅ **Zero concurrency conflicts** - each write touches different document
- ✅ **No 2MB limit** - documents stay small (100B-2KB each)
- ✅ **Simple reasoning** - no complex partitioning strategies
- ✅ **Cosmos DB default** - uses `/id` partition key automatically

**Trade-offs:**
- ❌ More RU consumption for reads (multiple queries vs single document)
- ❌ No transactional guarantees across documents
- ✅ **BUT:** Real-time apps already use SignalR push, minimizing read impact

---

## Implementation Plan

### Phase 1: Update DbContext (1 day)

#### File: `FacilitationDbContext.cs`

**Current (Owned Entities):**
```csharp
public class FacilitationDbContext : DbContext
{
    public DbSet<Meeting> Meetings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.OwnsMany(m => m.Stages, stage => { ... });
            entity.OwnsMany(m => m.Notes, note => { ... });
            // ... all child entities embedded
        });
    }
}
```

**New (Separate Documents):**
```csharp
public class FacilitationDbContext : DbContext
{
    // Restore all DbSets
    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<AgendaStage> AgendaStages { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Concern> Concerns { get; set; }
    public DbSet<ConcernVote> ConcernVotes { get; set; }
    public DbSet<AttendeeSession> AttendeeSessions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<QuestionResponse> QuestionResponses { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageResponse> MessageResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Simple configuration - each entity in own container
        // No partitioning - uses /id by default
        
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToContainer("Meetings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacilitatorToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AttendeeToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<AgendaStage>(entity =>
        {
            entity.ToContainer("AgendaStages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToContainer("Notes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Concern>(entity =>
        {
            entity.ToContainer("Concerns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConcernType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomText).HasMaxLength(500);
            entity.Property(e => e.ResponseText).HasMaxLength(500);
        });

        modelBuilder.Entity<ConcernVote>(entity =>
        {
            entity.ToContainer("ConcernVotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<AttendeeSession>(entity =>
        {
            entity.ToContainer("AttendeeSessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(30);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToContainer("Questions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ScaleMinLabel).HasMaxLength(100);
            entity.Property(e => e.ScaleMaxLabel).HasMaxLength(100);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.ToContainer("QuestionOptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OptionText).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<QuestionResponse>(entity =>
        {
            entity.ToContainer("QuestionResponses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttendeeSessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AnswerText).HasMaxLength(1000);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToContainer("Messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
        });

        modelBuilder.Entity<MessageResponse>(entity =>
        {
            entity.ToContainer("MessageResponses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reaction).HasMaxLength(10);
            entity.Property(e => e.FreeText).HasMaxLength(500);
        });
    }
}
```

---

### Phase 2: Update Handlers (1 day)

Most handlers already updated! Just need to revert from collection-based to DbSet-based operations.

#### Pattern Changes

**Add Operations:**
```csharp
// OLD (owned entities):
var meeting = await _context.Meetings.FindAsync(id);
meeting.Notes.Add(note);
await _context.SaveChangesAsync();

// NEW (separate documents):
var note = new Note { Id = Guid.NewGuid(), MeetingId = id, ... };
_context.Notes.Add(note);
await _context.SaveChangesAsync();
```

**Query Operations:**
```csharp
// OLD (owned entities):
var meeting = await _context.Meetings.FindAsync(id);
var notes = meeting.Notes.Where(n => n.IsPublic).ToList();

// NEW (separate documents):
var notes = await _context.Notes
    .Where(n => n.MeetingId == id && n.IsPublic)
    .ToListAsync();
```

**Update Operations:**
```csharp
// OLD (owned entities):
var meeting = await _context.Meetings.FindAsync(meetingId);
var note = meeting.Notes.FirstOrDefault(n => n.Id == noteId);
note.Content = newContent;
await _context.SaveChangesAsync();

// NEW (separate documents):
var note = await _context.Notes.FindAsync(noteId);
note.Content = newContent;
await _context.SaveChangesAsync();
```

**Delete Operations:**
```csharp
// OLD (owned entities):
var meeting = await _context.Meetings.FindAsync(meetingId);
meeting.Notes.Remove(note);
await _context.SaveChangesAsync();

// NEW (separate documents):
var note = await _context.Notes.FindAsync(noteId);
_context.Notes.Remove(note);
await _context.SaveChangesAsync();
```

#### Handlers to Update

**High Priority (Add Operations - currently touching Meeting doc):**
- ✅ `AddNoteHandler.cs` - revert to direct DbSet add
- ✅ `RaiseConcernHandler.cs` - revert to direct DbSet add
- ✅ `AddAgendaStageHandler.cs` - revert to direct DbSet add
- ✅ `CreateQuestionHandler.cs` - revert to direct DbSet add (and options)
- ✅ `CreateMessageHandler.cs` - revert to direct DbSet add (and options)
- ✅ `SubmitQuestionResponseHandler.cs` - revert to direct DbSet add
- ✅ `RespondToMessageHandler.cs` - revert to direct DbSet add
- ✅ `VoteConcernHandler.cs` - revert to direct DbSet add/update

**Medium Priority (Update Operations):**
- ✅ `UpdateNoteHandler.cs` - use FindAsync directly
- ✅ `AcknowledgeConcernHandler.cs` - use FindAsync directly
- ✅ `RespondToConcernHandler.cs` - use FindAsync directly
- ✅ `WithdrawConcernHandler.cs` - use FindAsync directly
- ✅ `CloseQuestionHandler.cs` - use FindAsync directly
- ✅ `CloseMessageHandler.cs` - use FindAsync directly
- ✅ `TriggerQuestionHandler.cs` - use FindAsync directly
- ✅ `StartStageHandler.cs` - query stages directly
- ✅ `EndStageHandler.cs` - query stages directly
- ✅ `EndMeetingHandler.cs` - query stages directly

**Low Priority (Delete Operations):**
- ✅ `DeleteNoteHandler.cs` - use FindAsync directly
- ✅ `DeleteAgendaStageHandler.cs` - query and reorder

**Query Handlers (Need individual queries):**
- ✅ `GetMeetingByIdHandler.cs` - remove .Include() calls
- ✅ `GetMeetingByTokenHandler.cs` - remove .Include() calls
- ✅ `GetConcernsByMeetingQueryHandler.cs` - direct query on Concerns
- ✅ `GetQuestionsByMeetingHandler.cs` - direct query on Questions
- ✅ `GetMessageHandlers.cs` - direct queries on Messages
- ✅ `GetQuestionResultsHandler.cs` - query Question + Responses separately
- ✅ `GetActiveQuestionsForAttendeeHandler.cs` - query Questions + Responses

---

### Phase 3: Optimize for Free Tier RU Usage (1 day)

#### 3.1 Index Optimization

**Only index fields you query on:**

```csharp
modelBuilder.Entity<Note>(entity =>
{
    entity.ToContainer("Notes");
    entity.HasNoDiscriminator();
    
    // Index for queries
    entity.HasIndex(n => n.MeetingId);
    entity.HasIndex(n => n.SessionId);
    
    // Don't index large content
    entity.Property(n => n.Content).HasNoIndex();
});

modelBuilder.Entity<QuestionResponse>(entity =>
{
    entity.ToContainer("QuestionResponses");
    entity.HasNoDiscriminator();
    
    // Index for queries
    entity.HasIndex(r => r.QuestionId);
    entity.HasIndex(r => r.AttendeeSessionId);
    
    // Don't index large text
    entity.Property(r => r.AnswerText).HasNoIndex();
});
```

**Savings:** 10-20% RU reduction on writes

---

#### 3.2 Query Optimization

**Add "since timestamp" queries for incremental sync:**

```csharp
// New query handler
public class GetMeetingUpdatesHandler : IRequestHandler<GetMeetingUpdatesQuery, MeetingUpdatesDto>
{
    public async ValueTask<MeetingUpdatesDto> Handle(...)
    {
        // Only fetch what changed since last sync
        var newNotes = await _context.Notes
            .Where(n => n.MeetingId == request.MeetingId && n.CreatedAt > request.SinceTime)
            .ToListAsync();
            
        var newConcerns = await _context.Concerns
            .Where(c => c.MeetingId == request.MeetingId && c.CreatedAt > request.SinceTime)
            .ToListAsync();
            
        // ... etc
        
        return new MeetingUpdatesDto(newNotes, newConcerns, ...);
    }
}
```

**Savings:** 50-70% RU reduction on polling queries

---

#### 3.3 Lazy Loading for Large Collections

**Don't auto-load responses:**

```csharp
// GetQuestionByIdHandler - lightweight version
public async ValueTask<QuestionDto> Handle(...)
{
    var question = await _context.Questions.FindAsync(request.QuestionId);
    
    // Convert to DTO without responses
    return new QuestionDto
    {
        Id = question.Id,
        Text = question.Text,
        // ... metadata only
        ResponseCount = await _context.QuestionResponses
            .CountAsync(r => r.QuestionId == request.QuestionId) // 1 RU
    };
}

// Separate handler for results (only when needed)
public class GetQuestionResultsHandler { ... } // Already exists
```

**Savings:** Load responses only when user clicks "View Results" → 10-15 RU saved per question view

---

#### 3.4 Client-Side Caching Strategy

**Update frontend to cache aggressively:**

```typescript
// Instead of polling every 5 seconds:
class MeetingCache {
    private lastSync: Date;
    private cachedNotes: Note[];
    
    async sync() {
        // Only fetch updates since last sync
        const updates = await api.getMeetingUpdates(this.meetingId, this.lastSync);
        this.cachedNotes.push(...updates.notes);
        this.lastSync = new Date();
    }
}

// Use SignalR for instant updates (zero RUs)
hubConnection.on("NoteAdded", (note) => {
    meetingCache.addNote(note); // Update cache instantly
});
```

**Savings:** 90% reduction in read RUs

---

### Phase 4: Add Monitoring & Limits (0.5 days)

#### 4.1 Document Size Monitoring

**Add telemetry to track document sizes:**

```csharp
public static class CosmosDbExtensions
{
    public static int EstimateSizeKB<T>(this T entity)
    {
        var json = JsonSerializer.Serialize(entity);
        return json.Length / 1024;
    }
}

// In handlers:
_logger.LogInformation("Meeting {Id} size: {Size}KB", meeting.Id, meeting.EstimateSizeKB());
```

---

#### 4.2 Response Limits

**Add limits to prevent unbounded growth:**

```csharp
// In SubmitQuestionResponseHandler:
public async ValueTask<Guid> Handle(...)
{
    var responseCount = await _context.QuestionResponses
        .CountAsync(r => r.QuestionId == request.QuestionId);
        
    if (responseCount >= 500) // Safety limit
        throw new InvalidOperationException("Question response limit reached");
        
    // ... proceed with submission
}

// Similar limits for:
// - Messages per meeting: 100
// - Questions per meeting: 50
// - Total responses per meeting: 1000
```

---

#### 4.3 RU Consumption Dashboard

**Add endpoint to track RU usage:**

```csharp
public class CosmosDbMetricsHandler : IRequestHandler<GetCosmosMetricsQuery, CosmosMetricsDto>
{
    public async ValueTask<CosmosMetricsDto> Handle(...)
    {
        // Cosmos SDK exposes RequestCharge
        // Aggregate and return to admin dashboard
        
        return new CosmosMetricsDto
        {
            TotalRUs24h = ...,
            AverageRUsPerMinute = ...,
            PeakRUsBurst = ...
        };
    }
}
```

---

### Phase 5: Update Tests (0.5 days)

#### Revert Test Changes

Tests currently load Meeting and navigate collections. Revert to direct DbSet queries:

```csharp
// OLD (owned entities):
var savedMeeting = await context.Meetings.FirstOrDefaultAsync(m => m.Id == meetingId);
var savedNote = savedMeeting?.Notes.FirstOrDefault(n => n.Id == noteId);

// NEW (separate documents):
var savedNote = await context.Notes.FindAsync(noteId);
```

**Files to update:**
- `MeetingWorkflowTests.cs` - revert all test assertions

---

## RU Capacity Planning

### Expected Load (Free Tier: 1000 RU/s)

| Meeting Type | Attendees | Duration | Total RUs | Avg RU/s | Peak RU/s | Status |
|--------------|-----------|----------|-----------|----------|-----------|--------|
| **Small** | 5 | 15 min | 415 | 0.5 | 5 | ✅ Perfect |
| **Medium** | 15 | 2 hours | 20,100 | 2.8 | 25 | ✅ Great |
| **Large** | 50 | 3 hours | 61,650 | 5.7 | 75 | ✅ Good |
| **Conference** | 200 | 4 hours | 467,750 | 32.5 | 250 | ⚠️ Needs optimization |

**Concurrent Capacity (with optimizations):**
- 5-10 small meetings simultaneously
- 2-3 medium meetings simultaneously
- 1 large meeting comfortably
- Conference events need additional caching/throttling

---

## Migration Strategy

### Development Environment

1. **Create new branch:** `feature/cosmos-separate-documents`
2. **Update DbContext** → Phase 1
3. **Update handlers** → Phase 2
4. **Run tests** → Verify all pass
5. **Add optimizations** → Phase 3
6. **Deploy to dev Cosmos DB** → Test real-world performance

### Production Deployment

**Option A: Clean Migration (Recommended)**
1. Export existing data (if any)
2. Delete old containers
3. Deploy new DbContext configuration
4. Re-import data (documents will be in new structure)

**Option B: Dual-Write (Zero Downtime)**
1. Deploy new code with both configs
2. Write to both old & new containers
3. Migrate data in background
4. Switch reads to new containers
5. Remove old containers

---

## Success Criteria

### Performance Targets
- ✅ Zero 412 concurrency errors under load
- ✅ 20 attendees can submit responses simultaneously without conflicts
- ✅ Average meeting (15 attendees, 2 hours) uses <5% of free tier RU/s
- ✅ Peak bursts stay under 100 RU/s for typical usage

### Monitoring Metrics
- Document sizes stay under 10KB average
- Query response times <100ms (p95)
- Write response times <50ms (p95)
- RU consumption dashboard shows trends

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Multi-document reads slower** | Medium | Use SignalR push instead of polling |
| **No transactional guarantees** | Low | App logic already handles eventual consistency |
| **Higher RU on reads** | Medium | Implement caching + incremental sync |
| **Complex query patterns** | Low | Most queries by MeetingId (simple) |

---

## Future Enhancements (Post-MVP)

1. **Partitioning by MeetingId** - When scaling beyond free tier
2. **Redis cache layer** - For hot meeting data
3. **CQRS with Event Sourcing** - For audit trail & advanced analytics
4. **Archive cold meetings** - Move ended meetings to cheaper storage
5. **RU auto-scaling** - Upgrade tier during large events

---

## Estimated Timeline

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: DbContext | 1 day | None |
| Phase 2: Handlers | 1 day | Phase 1 |
| Phase 3: Optimizations | 1 day | Phase 2 |
| Phase 4: Monitoring | 0.5 days | Phase 3 |
| Phase 5: Tests | 0.5 days | Phase 2 |
| **Total** | **4 days** | - |

---

## Appendix: Key Files to Modify

### Infrastructure Layer
- `FacilitationDbContext.cs` - Remove OwnsMany, add ToContainer
- ~30 handler files in `Handlers/` - Revert from collection to DbSet operations

### Test Layer
- `MeetingWorkflowTests.cs` - Update test assertions

### Configuration
- `appsettings.json` - No changes needed (same connection string)

---

## Next Steps

1. ✅ Review this plan with team
2. ⏳ Create feature branch
3. ⏳ Implement Phase 1 (DbContext)
4. ⏳ Implement Phase 2 (Handlers) 
5. ⏳ Run full test suite
6. ⏳ Deploy to dev environment
7. ⏳ Load test with 50 concurrent users
8. ⏳ Implement Phase 3 (Optimizations)
9. ⏳ Production deployment

---

**Document Version:** 1.0  
**Last Updated:** February 18, 2026  
**Status:** Ready for Implementation
