# Facilitation Assistant - Backend Implementation

## Implementation Summary

The backend architecture has been fully implemented according to the architecture document and implementation plan. The solution follows Clean Architecture principles with three main projects:

### Projects Structure

#### 1. **FacilitationAssistant.Core** (Domain Layer)
- **Domain Entities & Aggregates:**
  - `Meeting` - Aggregate root managing all meeting operations
  - `AgendaStage` - Meeting stages with timing
  - `Attendee` - Meeting participants
  - `Poll`, `PollOption`, `Vote` - Polling system
  - `Message` - Announcements and questions
  - `Concern` - Feedback and concerns tracking
  - `Note` - Notes system (both facilitator and attendee)

- **Application Layer (CQRS):**
  - Commands: CreateMeeting, AddAgendaStage, StartMeeting, StartStage, CreatePoll, OpenPoll, Vote, SendMessage, RaiseConcern, AddNote, JoinMeeting
  - Queries: GetMeetingState, GetMyNotes
  - Notifications: MeetingUpdatedNotification
  - DTOs: MeetingStateDto with all child DTOs

- **Interfaces:**
  - `IMeetingRepository` - Repository pattern
  - `IDateTimeProvider` - Testable time provider
  - `ICurrentUserService` - User context abstraction

#### 2. **FacilitationAssistant.Infrastructure** (Data Access)
- **Persistence:**
  - `AppDbContext` - EF Core DbContext
  - Entity configurations with Fluent API
  - `MeetingRepository` - Uses `IDbContextFactory<T>` for short-lived contexts (critical for Blazor Server)
  - SQLite database with migration support

- **Services:**
  - `SystemClock` - Production time provider

#### 3. **FacilitationAssistant.Web** (Presentation)
- **SignalR:**
  - `MeetingHub` - Real-time communication hub
  - `MeetingUpdatedNotificationHandler` - Broadcasts state changes to connected clients

- **Services:**
  - `CurrentUserService` - Extracts user context from HTTP

- **Configuration:**
  - Complete DI setup in `Program.cs`
  - SignalR hub mapping at `/meetingHub`
  - DbContextFactory registration (prevents Blazor Server threading issues)

## Key Architectural Decisions

### 1. DbContext Factory Pattern
Uses `IDbContextFactory<AppDbContext>` instead of Scoped DbContext to avoid threading issues in long-lived Blazor Server circuits.

```csharp
// In repositories
await using var context = await _contextFactory.CreateDbContextAsync(ct);
```

### 2. Real-Time Synchronization Strategy

**Push Strategy (Public State):**
- Meeting state changes publish `MeetingUpdatedNotification`
- Handler fetches fresh state and broadcasts via SignalR
- Clients receive complete state via `ReceiveState` event

**Pull Strategy (Private Data):**
- Private notes are NOT included in broadcast
- Clients query `GetMyNotesQuery` separately
- Prevents data leaks

### 3. Command Pattern with MediatR
All state mutations go through commands:
1. UI sends command
2. Handler loads aggregate
3. Domain method validates and mutates
4. Save to DB
5. Publish notification
6. SignalR broadcasts to group

### 4. Domain-Driven Design
- Rich domain models with business logic
- Aggregate boundaries enforced
- Value objects and enums for type safety

## Database Schema

Entities:
- `Meetings` (Aggregate Root)
  - `AgendaStages`
  - `Attendees`
  - `Polls`
    - `PollOptions`
    - `Votes`
  - `Messages`
  - `Concerns`
  - `Notes`

All relationships use cascade delete. Meeting is the aggregate root owning all child entities.

## Running the Application

### 1. Apply Database Migration
```bash
cd src/FacilitationAssistant.Web
dotnet ef database update
```

### 2. Run the Application
```bash
cd src/FacilitationAssistant.Web
dotnet run
```

### 3. SignalR Connection (for UI)
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/meetingHub")
    .build();

await connection.start();
await connection.invoke("JoinMeeting", meetingId);

connection.on("ReceiveState", (state) => {
    // Update UI with new state
});
```

## Next Steps for UI Development

1. **Create Meeting Page:**
   - Form to create meeting
   - Use `CreateMeetingCommand`
   - Generate facilitator URL with `FacilitatorKey`

2. **Facilitator View:**
   - Display agenda stages
   - Start/Complete stage buttons
   - Create polls, messages, manage concerns
   - View all notes (public + private)

3. **Attendee View:**
   - Join with meeting ID
   - View current stage and timer
   - Vote on polls
   - Send messages/concerns
   - Take personal notes

4. **Real-Time Updates:**
   - Connect to SignalR hub on component mount
   - Subscribe to `ReceiveState`
   - Update component state reactively

## Testing the Backend

Example API usage (for testing):

```csharp
// Create meeting
var createResult = await mediator.Send(new CreateMeetingCommand("Sprint Planning"));
var meetingId = createResult.Value;

// Add stages
await mediator.Send(new AddAgendaStageCommand(meetingId, "Introduction", null, 5, 0));
await mediator.Send(new AddAgendaStageCommand(meetingId, "Sprint Goals", null, 15, 1));

// Start meeting
await mediator.Send(new StartMeetingCommand(meetingId));

// Join as attendee
await mediator.Send(new JoinMeetingCommand(meetingId, "session123", "John Doe"));

// Get state
var state = await mediator.Send(new GetMeetingStateQuery(meetingId));
```

## Technology Stack

- **.NET 9**
- **Entity Framework Core 9** with SQLite
- **MediatR 12.4** for CQRS
- **SignalR** for real-time communication
- **Blazor Server** (UI framework)

## Architecture Compliance

✅ All items from architecture.md implemented
✅ All items from plan.md completed
✅ Clean Architecture layers enforced
✅ SOLID principles applied
✅ Async/await throughout
✅ Proper error handling with Result pattern
✅ Domain encapsulation with private setters
✅ Repository pattern with abstraction
✅ CQRS with MediatR
✅ Real-time sync via SignalR
✅ Factory pattern for DbContext (Blazor Server best practice)
