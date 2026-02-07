# Feature Integration & Dependency Matrix

## Purpose

This document maps dependencies between features, identifies integration points, and highlights risks of feature interaction failures.

---

## Feature Dependency Graph

```
Meeting Creation (Foundation)
    ├─→ Agenda Management (requires meeting ID, stage definitions)
    │   ├─→ Real-Time Timing (requires stage start timestamps)
    │   └─→ Polling System (stage-associated questions)
    │
    ├─→ Attendee Identity (requires meeting ID for session scoping)
    │   ├─→ Concerns System (requires session ID for ownership)
    │   ├─→ Notes System (requires session ID for ownership)
    │   └─→ Polling System (requires session ID for response attribution)
    │
    ├─→ Real-Time Sync (requires meeting ID for WebSocket channel)
    │   ├─→ All Features (all depend on sync for updates)
    │
    └─→ Meeting Summary (requires meeting ID, agenda, session data)
```

---

## Integration Points & Risks

### 1. Meeting Creation → Agenda Management
**Integration**: Meeting creation generates agenda structure; agenda management controls stage lifecycle

**Data Flow**:
- Meeting Creation: Generates `meeting_id`, stores initial `AgendaStage[]` via EF Core repository
- Agenda Management: Reads stages via query handler, updates `current_stage_id`, `stage_start_timestamp` via commands

**Failure Scenarios**:
- **What if agenda has 0 stages when meeting starts?**
  - Risk: Agenda management has no stages to activate
  - Mitigation: Validation in meeting creation (AC9: minimum 1 stage required)
  
- **What if stage durations sum to 0?**
  - Risk: Overall progress bar shows 100% immediately
  - Mitigation: Meeting creation calculates total duration, warns if suspicious

**Testing**:
- Integration test: Create meeting → Add stages → Start meeting → Verify first stage activates

---

### 2. Agenda Management → Polling System
**Integration**: Questions can auto-trigger when stages start; stage transitions affect active questions

**Data Flow**:
- Agenda Management: Broadcasts `stage_started` event via MediatR notification handler
- Polling System: Subscribes to event via INotificationHandler, checks if questions have `trigger: stage_start`, shows questions to attendees via SignalR hub

**Failure Scenarios**:
- **What if stage starts but question fails to trigger?**
  - Risk: Data not collected, facilitator unaware
  - Mitigation: Polling system logs trigger failures, facilitator sees "Question failed to show - Trigger manually"

- **What if stage ends while attendee is answering question?**
  - Risk: Response lost, attendee frustrated
  - Mitigation: Polling system saves response to stage where question originated, shows "Stage changed - Your answer was saved"

**Testing**:
- Integration test: Define stage-associated question → Start stage → Verify question appears
- Edge case test: End stage while attendee has unsaved answer → Verify response saved to correct stage

---

### 3. Attendee Identity → Concerns System
**Integration**: Concerns are owned by session_id; concern display shows attendee display name or "Attendee #X"

**Data Flow**:
- Attendee Identity: Generates `session_id`, stores `display_name`, assigns `attendee_number` in PostgreSQL session table
- Concerns System: Creates concern with `session_id` via command handler, queries attendee identity repository for display

**Failure Scenarios**:
- **What if session_id is not found when displaying concern?**
  - Risk: Concern shows "Unknown" author, confusing
  - Mitigation: Concerns system caches attendee identity at concern creation time (denormalized)

- **What if attendee changes display name after raising concern?**
  - Risk: Concern shows old name, identity confusion
  - Mitigation: Attendee identity spec AC8: retroactive update of all contributions

**Testing**:
- Integration test: Set display name → Raise concern → Verify concern shows name
- Edge case test: Raise concern → Change name → Verify concern updates to new name

---

### 4. Real-Time Sync → All Features
**Integration**: Every feature broadcasts state changes via sync layer; all features subscribe to sync updates

**Data Flow**:
- Feature emits event: `await _mediator.Publish(new ConcernRaisedEvent(...))`
- MediatR: Invokes all registered INotificationHandler<ConcernRaisedEvent>
- SignalR Handler: Broadcasts event to all connected Blazor circuits via IMeetingHub
- Subscribers: Blazor components receive hub invocation, update state, trigger StateHasChanged()

**Failure Scenarios**:
- **What if WebSocket fails during critical event (stage transition)?**
  - Risk: Attendees desync, see stale stage
  - Mitigation: SignalR long polling fallback (automatic), full state snapshot on Blazor circuit reconnect via OnInitializedAsync

- **What if event arrives out of order (due to network jitter)?**
  - Risk: State corruption (e.g., stage ends before it starts)
  - Mitigation: SignalR guarantees message order within a connection; MediatR command handlers are transactional via EF Core DbContext

**Testing**:
- Integration test: Simulate SignalR connection failure → Verify long polling fallback activates
- Stress test: Send 100 events in 1 second → Verify all Blazor circuits apply in correct order

---

### 5. Polling System → Attendee Identity
**Integration**: Poll responses are attributed to session_id; results show display name or "Attendee #X"

**Data Flow**:
- Polling System: Stores response with `session_id` via command handler → EF Core repository
- Attendee Identity: Provides display name lookup via query handler
- Facilitator views results: "Alice voted 'Yes'", "Attendee #5 voted 'No'"

**Failure Scenarios**:
- **What if session_id is deleted (attendee leaves) but response remains?**
  - Risk: Orphaned response, cannot display author
  - Mitigation: Soft-delete sessions in PostgreSQL (mark inactive via EF Core, don't delete until meeting ends)

- **What if two attendees have same display name?**
  - Risk: Confusion about who answered what
  - Mitigation: Attendee identity spec AC9: suffix with session ID (e.g., "John (A5)")

**Testing**:
- Integration test: Answer question → Verify response shows display name
- Edge case test: Two attendees with same name → Verify both shown with distinct IDs

---

### 6. Notes System → Attendee Identity
**Integration**: Notes are owned by session_id; note editing/deletion requires ownership check

**Data Flow**:
- Notes System: Stores note with `session_id`, `visibility: 'public' | 'private'` via EF Core repository
- Attendee Identity: Provides session validation via query handler
- UI: Blazor component shows "Edit"/"Delete" buttons only on notes where `note.session_id === current_session_id`

**Failure Scenarios**:
- **What if session_id is spoofed (malicious client)?**
  - Risk: Attendee edits/deletes others' notes
  - Mitigation: Server-side validation in command handler, reject if `note.session_id !== request.session_id` (Blazor Server enforces server-side validation)

- **What if note is public but attendee tries to edit after meeting ends?**
  - Risk: Post-meeting vandalism
  - Mitigation: Notes system locks all edits after meeting ends via business rule in command handler (read-only summary)

**Testing**:
- Integration test: Create note via command → Try to edit from different session → Verify 403 Forbidden from command handler
- Security test: Attempt to spoof session_id in command payload → Verify server rejects via validation pipeline

---

### 7. Messaging → Real-Time Sync
**Integration**: Messages broadcast to all attendees; real-time delivery critical for notifications

**Data Flow**:
- Messaging: Facilitator sends message via command handler → SignalR hub broadcasts to all Blazor circuits
- Real-Time Sync: Delivers message via SignalR hub invocation
- Attendees: Blazor components update state, show notification, store in in-memory message history

**Failure Scenarios**:
- **What if attendee is disconnected when message sent?**
  - Risk: Missed message, attendee out of loop
  - Mitigation: On Blazor circuit reconnect, query handler returns missed messages from server-side state (message history query)

- **What if message delivery fails to some attendees?**
  - Risk: Partial delivery, inconsistent meeting state
  - Mitigation: Facilitator component sees delivery status: "Message sent to 10/12 attendees (2 offline)" tracked via SignalR connection state

**Testing**:
- Integration test: Send message via command → Verify all connected Blazor circuits receive within 2 seconds via SignalR
- Edge case test: Disconnect attendee → Send message → Reconnect → Verify message delivered via state sync

---

### 8. Meeting Summary → All Features
**Integration**: Summary aggregates data from all features; must query concerns, notes, poll results, agenda

**Data Flow**:
- Meeting Summary: Query handler fetches all meeting data from PostgreSQL via EF Core repositories
- Concerns System: Repository provides concerns with vote counts, status
- Notes System: Repository provides public notes (filters out private via LINQ query for attendee summary)
- Polling System: Repository provides poll results, response counts
- Agenda Management: Repository provides stage durations, actual vs planned time

**Failure Scenarios**:
- **What if summary generation fails (database query timeout)?**
  - Risk: Users see "Summary not available" error
  - Mitigation: Async generation via background service, show "Generating summary..." progress, retry on failure with exponential backoff

- **What if private notes leak into attendee summary?**
  - Risk: Privacy breach, confidential info exposed
  - Mitigation: Notes repository filters by `visibility = 'public'` AND `meeting_ended = true` in LINQ query (enforced at query handler level)

**Testing**:
- Integration test: End meeting via command → Generate summary via query → Verify all data present from EF Core
- Privacy test: Create private note → Execute attendee summary query → Verify note not visible via LINQ filter
- Performance test: Meeting with 500 concerns, 200 notes → Verify summary EF Core query loads in <5 seconds

---

## Cross-Feature Workflows

### Workflow 1: Facilitator Creates Meeting & First Stage Transition
1. **Meeting Creation**: Facilitator creates meeting → Command handler persists to PostgreSQL → Returns links
2. **Agenda Management**: Facilitator adds 3 stages (10m, 15m, 10m) → Command handler persists
3. **Meeting Start**: Facilitator clicks "Start Meeting" → Attendee link goes live
4. **Attendee Identity**: Attendees join → Blazor circuit established → Session created in PostgreSQL, assigned `session_id`, `attendee_number`
5. **Real-Time Sync**: All Blazor circuits connected via SignalR automatically
6. **Stage Transition**: Facilitator starts Stage 1 → Command handler processes → MediatR publishes `StageStartedEvent` → SignalR hub broadcasts to all circuits
7. **Polling System**: Stage 1 has associated question → Event handler auto-triggers → SignalR invokes `ShowQuestion` on attendee circuits
8. **Concerns System**: Attendee raises concern "Topic unclear" via command → SignalR notifies facilitator circuit

**Integration Points**: 7 features interact in sequence

**Failure Point**: If SignalR connection fails at step 6, attendees don't see stage transition
- **Mitigation**: Long polling fallback (automatic), Blazor reconnection prompt

---

### Workflow 2: Late Joiner Catches Up
1. **Meeting Creation**: Meeting already active (Stage 2 of 3)
2. **Attendee Identity**: New attendee accesses link → Blazor circuit established → Session created in PostgreSQL → Assigned "Attendee #8"
3. **Real-Time Sync**: Attendee circuit connects → OnInitializedAsync queries for full state snapshot from server
4. **Agenda Management**: Component renders Stage 2 active, Stage 1 completed, Stage 3 not started
5. **Messaging**: Component queries for "5 messages sent earlier" (collapsed summary) from server state
6. **Concerns System**: Component loads 3 active concerns with votes via EF Core query
7. **Notes System**: Component loads 2 public notes from earlier stages via EF Core query

**Integration Points**: 6 features provide onboarding context

**Failure Point**: If state snapshot is incomplete, attendee sees partial data
- **Mitigation**: Full snapshot loaded via comprehensive query handler, integrity check enforced via unit tests

---

### Workflow 3: Meeting End & Summary Generation
1. **Agenda Management**: Facilitator clicks "End Meeting" → Command handler sets meeting status = 'ended' in PostgreSQL
2. **Real-Time Sync**: MediatR publishes `MeetingEndedEvent` → SignalR hub broadcasts to all circuits → Blazor components update
3. **Meeting Summary**: Background service job starts, executes comprehensive query handler
4. **Concerns System**: Repository query provides all concerns, vote counts, acknowledgment status
5. **Notes System**: Repository query provides public notes only (LINQ filters by visibility = 'public')
6. **Polling System**: Repository query provides all poll results, response counts, aggregated data
7. **Messaging**: Service queries message history (facilitator and attendee responses) from server state
8. **Meeting Summary**: Service renders HTML summary, persists to read-only link stored in PostgreSQL
9. **Attendee Identity**: Background cleanup job marks sessions inactive in PostgreSQL after 48 hours

**Integration Points**: 8 features contribute to summary

**Failure Point**: If one feature query fails, partial summary generated
- **Mitigation**: Best-effort summary via try-catch in query handler, show "Some data unavailable" warning, log exceptions

---

## Integration Testing Strategy

### Phase 1: Unit Tests (Feature Isolation)
- Each feature tested independently with mocked dependencies (Moq for repositories, IMediator)
- Example: Concerns system command handler tested with mocked IAttendeeRepository, mocked IHubContext

### Phase 2: Integration Tests (Feature Pairs)
- Test interactions between two features with real EF Core in-memory database
- Example: Agenda + Polling (stage trigger), Identity + Concerns (ownership validation)

### Phase 3: End-to-End Tests (Full Workflows)
- Test complete user journeys involving multiple features with PostgreSQL test database
- Example: Create meeting → Add stages → Start → Raise concern → End → View summary (via Blazor bUnit tests)

### Phase 4: Stress Tests (Failure Scenarios)
- Simulate failures at integration points with Testcontainers PostgreSQL
- Example: SignalR connection fails during stage transition → Verify fallback works via bUnit component test

---

## Known Integration Debt

### 1. Eventual Consistency Issues
**Problem**: Real-time sync via SignalR is not transactional; events may arrive out of order (rare, but possible with long polling)
**Impact**: Brief UI inconsistencies (1-2 second window)
**Mitigation**: SignalR guarantees message order per connection; MediatR command handlers use EF Core transactions
**Long-term Fix**: Event sourcing with guaranteed ordering, or implement sequence numbers in event payloads

### 2. Session ID Propagation
**Problem**: Every feature needs session_id, must be consistent across Blazor circuit and commands
**Impact**: Code duplication, potential for bugs
**Mitigation**: Scoped service ICurrentUserSession injected into command handlers via DI, populated from Blazor circuit context
**Long-term Fix**: Unified authentication layer (even without login), or ASP.NET Core authentication middleware

### 3. Circular Dependency: Sync ↔ Features
**Problem**: Features publish events via MediatR, SignalR handlers broadcast events back to Blazor components
**Impact**: Hard to test in isolation, tight coupling between notification handlers and hub context
**Mitigation**: Dependency injection for IHubContext<MeetingHub>, unit test with mocked hub context
**Long-term Fix**: Message queue (RabbitMQ, Azure Service Bus) decouples features, or use MassTransit for abstraction

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-18  
**Status**: Draft - Integration Architecture
