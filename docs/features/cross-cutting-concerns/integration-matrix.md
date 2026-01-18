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
- Meeting Creation: Generates `meeting_id`, stores initial `AgendaStage[]`
- Agenda Management: Reads stages, updates `current_stage_id`, `stage_start_timestamp`

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
- Agenda Management: Broadcasts `stage_started` event with `stage_id`
- Polling System: Listens for event, checks if questions have `trigger: stage_start`, shows questions to attendees

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
- Attendee Identity: Generates `session_id`, stores `display_name`, assigns `attendee_number`
- Concerns System: Creates concern with `session_id`, queries attendee identity for display

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
- Feature emits event: `sync.broadcast({ type: 'concern_raised', payload: {...} })`
- Real-Time Sync: Sends event to all connected clients via WebSocket
- Subscribers: Update local state when event received

**Failure Scenarios**:
- **What if WebSocket fails during critical event (stage transition)?**
  - Risk: Attendees desync, see stale stage
  - Mitigation: Polling fallback (AC2), full state snapshot on reconnect (AC5)

- **What if event arrives out of order (due to network jitter)?**
  - Risk: State corruption (e.g., stage ends before it starts)
  - Mitigation: Sequence numbers on all events, clients apply in order (edge case #6 in real-time-sync)

**Testing**:
- Integration test: Simulate WebSocket failure → Verify polling fallback activates
- Stress test: Send 100 events in 1 second → Verify all clients apply in correct order

---

### 5. Polling System → Attendee Identity
**Integration**: Poll responses are attributed to session_id; results show display name or "Attendee #X"

**Data Flow**:
- Polling System: Stores response with `session_id`
- Attendee Identity: Provides display name lookup
- Facilitator views results: "Alice voted 'Yes'", "Attendee #5 voted 'No'"

**Failure Scenarios**:
- **What if session_id is deleted (attendee leaves) but response remains?**
  - Risk: Orphaned response, cannot display author
  - Mitigation: Soft-delete sessions (mark inactive, don't delete until meeting ends)

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
- Notes System: Stores note with `session_id`, `visibility: 'public' | 'private'`
- Attendee Identity: Provides session validation
- UI: Shows "Edit"/"Delete" buttons only on notes where `note.session_id === current_session_id`

**Failure Scenarios**:
- **What if session_id is spoofed (malicious client)?**
  - Risk: Attendee edits/deletes others' notes
  - Mitigation: Server-side validation, reject if `note.session_id !== request.session_id`

- **What if note is public but attendee tries to edit after meeting ends?**
  - Risk: Post-meeting vandalism
  - Mitigation: Notes system locks all edits after meeting ends (read-only summary)

**Testing**:
- Integration test: Create note → Try to edit from different session → Verify 403 Forbidden
- Security test: Spoof session_id in API request → Verify server rejects

---

### 7. Messaging → Real-Time Sync
**Integration**: Messages broadcast to all attendees; real-time delivery critical for notifications

**Data Flow**:
- Messaging: Facilitator sends message → Sync broadcasts to all attendees
- Real-Time Sync: Delivers message via WebSocket or polling
- Attendees: Show notification, store in message history

**Failure Scenarios**:
- **What if attendee is disconnected when message sent?**
  - Risk: Missed message, attendee out of loop
  - Mitigation: On reconnect, sync sends missed messages (message history query)

- **What if message delivery fails to some attendees?**
  - Risk: Partial delivery, inconsistent meeting state
  - Mitigation: Facilitator sees delivery status: "Message sent to 10/12 attendees (2 offline)"

**Testing**:
- Integration test: Send message → Verify all connected attendees receive within 2 seconds
- Edge case test: Disconnect attendee → Send message → Reconnect → Verify message delivered

---

### 8. Meeting Summary → All Features
**Integration**: Summary aggregates data from all features; must query concerns, notes, poll results, agenda

**Data Flow**:
- Meeting Summary: Queries database for all meeting data
- Concerns System: Provides concerns with vote counts, status
- Notes System: Provides public notes (filters out private for attendee summary)
- Polling System: Provides poll results, response counts
- Agenda Management: Provides stage durations, actual vs planned time

**Failure Scenarios**:
- **What if summary generation fails (database query timeout)?**
  - Risk: Users see "Summary not available" error
  - Mitigation: Async generation, show "Generating summary..." progress, retry on failure

- **What if private notes leak into attendee summary?**
  - Risk: Privacy breach, confidential info exposed
  - Mitigation: Notes system filters by `visibility = 'public'` AND `meeting_ended = true`

**Testing**:
- Integration test: End meeting → Generate summary → Verify all data present
- Privacy test: Create private note → View attendee summary → Verify note not visible
- Performance test: Meeting with 500 concerns, 200 notes → Verify summary loads in <5 seconds

---

## Cross-Feature Workflows

### Workflow 1: Facilitator Creates Meeting & First Stage Transition
1. **Meeting Creation**: Facilitator creates meeting → Receives links
2. **Agenda Management**: Facilitator adds 3 stages (10m, 15m, 10m)
3. **Meeting Start**: Facilitator clicks "Start Meeting" → Attendee link goes live
4. **Attendee Identity**: Attendees join → Assigned session_id, attendee_number
5. **Real-Time Sync**: All attendees connected via WebSocket
6. **Stage Transition**: Facilitator starts Stage 1 → Sync broadcasts `stage_started` event
7. **Polling System**: Stage 1 has associated question → Auto-triggered → Attendees see question
8. **Concerns System**: Attendee raises concern "Topic unclear" → Facilitator sees it

**Integration Points**: 7 features interact in sequence

**Failure Point**: If WebSocket fails at step 6, attendees don't see stage transition
- **Mitigation**: Polling fallback, manual refresh prompt

---

### Workflow 2: Late Joiner Catches Up
1. **Meeting Creation**: Meeting already active (Stage 2 of 3)
2. **Attendee Identity**: New attendee accesses link → Session created → Assigned "Attendee #8"
3. **Real-Time Sync**: Attendee connects → Receives full state snapshot
4. **Agenda Management**: Attendee sees Stage 2 active, Stage 1 completed, Stage 3 not started
5. **Messaging**: Attendee sees "5 messages sent earlier" (collapsed summary)
6. **Concerns System**: Attendee sees 3 active concerns with votes
7. **Notes System**: Attendee sees 2 public notes from earlier stages

**Integration Points**: 6 features provide onboarding context

**Failure Point**: If state snapshot is incomplete, attendee sees partial data
- **Mitigation**: Full snapshot includes all features, integrity check before send

---

### Workflow 3: Meeting End & Summary Generation
1. **Agenda Management**: Facilitator clicks "End Meeting" → Meeting status = 'ended'
2. **Real-Time Sync**: Broadcasts `meeting_ended` event → All clients update
3. **Meeting Summary**: Async job starts, queries all features
4. **Concerns System**: Provides all concerns, vote counts, acknowledgment status
5. **Notes System**: Provides public notes only (filters private)
6. **Polling System**: Provides all poll results, response counts, aggregated data
7. **Messaging**: Provides message history (facilitator and attendee responses)
8. **Meeting Summary**: Renders HTML summary, saves to read-only link
9. **Attendee Identity**: Sessions marked inactive after 48 hours

**Integration Points**: 8 features contribute to summary

**Failure Point**: If one feature query fails, partial summary generated
- **Mitigation**: Best-effort summary, show "Some data unavailable" warning

---

## Integration Testing Strategy

### Phase 1: Unit Tests (Feature Isolation)
- Each feature tested independently with mocked dependencies
- Example: Concerns system tested with mocked attendee identity, mocked sync

### Phase 2: Integration Tests (Feature Pairs)
- Test interactions between two features
- Example: Agenda + Polling (stage trigger), Identity + Concerns (ownership)

### Phase 3: End-to-End Tests (Full Workflows)
- Test complete user journeys involving multiple features
- Example: Create meeting → Add stages → Start → Raise concern → End → View summary

### Phase 4: Stress Tests (Failure Scenarios)
- Simulate failures at integration points
- Example: WebSocket fails during stage transition → Verify fallback works

---

## Known Integration Debt

### 1. Eventual Consistency Issues
**Problem**: Real-time sync is not transactional; events may arrive out of order
**Impact**: Brief UI inconsistencies (1-2 second window)
**Mitigation**: Sequence numbers, client-side reordering
**Long-term Fix**: Event sourcing with guaranteed ordering

### 2. Session ID Propagation
**Problem**: Every feature needs session_id, but passed differently (HTTP header, WebSocket frame)
**Impact**: Code duplication, potential for bugs
**Mitigation**: Middleware injects session_id into all requests
**Long-term Fix**: Unified authentication layer (even without login)

### 3. Circular Dependency: Sync ↔ Features
**Problem**: Features emit events via sync, sync delivers events to features
**Impact**: Hard to test in isolation, tight coupling
**Mitigation**: Event bus abstraction, dependency injection
**Long-term Fix**: Message queue (RabbitMQ, Kafka) decouples features

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-18  
**Status**: Draft - Integration Architecture
