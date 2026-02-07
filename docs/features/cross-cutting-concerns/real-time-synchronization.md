# Feature: Real-Time Synchronization & State Management

## Problem Statement

Multiple attendees and the facilitator must see consistent meeting state (stage transitions, timer updates, questions, concerns) with minimal latency. Network failures, browser tab switches, and concurrent actions create race conditions that can desynchronize meeting state, confusing participants and undermining facilitator control.

## User Stories

- **US1**: As an attendee, I want to see stage transitions within 2 seconds so that I'm not confused when the facilitator moves forward
- **US2**: As a facilitator, I want my actions to sync to all attendees reliably so that everyone sees the same meeting state
- **US3**: As an attendee, I want automatic reconnection after network interruption so that I don't lose access to the meeting
- **US4**: As a facilitator, I want conflict resolution when I control the meeting from two devices so that my latest action always wins
- **US5**: As an attendee, I want visible sync status so that I know if I'm seeing stale data
- **US6**: As a facilitator, I want to know how many attendees are currently connected so that I can wait for stragglers

## Acceptance Criteria

### WebSocket Connection Management

- **AC1**: Given I access a meeting link, When the page loads, Then a SignalR connection is established within 3 seconds or fallback to long polling occurs
- **AC2**: Given SignalR connection fails, When fallback to long polling activates, Then I see a warning indicator "Limited connectivity - using fallback mode"
- **AC3**: Given I lose network connection, When connection is lost, Then I see "Disconnected - Reconnecting..." message with retry countdown
- **AC4**: Given I was disconnected, When network returns, Then reconnection attempts occur at: 1s, 2s, 5s, 10s, 30s intervals (exponential backoff)
- **AC5**: Given I reconnect successfully, When connection is reestablished, Then I receive full current meeting state snapshot to resync via Blazor Server state management

### State Synchronization Timing

- **AC6**: Given the facilitator starts a stage, When the action is broadcast, Then all connected attendees see the change within 2 seconds
- **AC7**: Given an attendee raises a concern, When the concern is submitted, Then the facilitator sees it within 2 seconds
- **AC8**: Given timers are running, When I view the interface, Then timer updates occur every 1 second with <100ms jitter
- **AC9**: Given I am viewing the meeting, When I switch browser tabs and return, Then timers catch up to correct time (no freeze on inactive tabs)

### Concurrent Action Handling

- **AC10**: Given I am the facilitator with two browser tabs open, When I start a stage in Tab A and start a different stage in Tab B simultaneously, Then the last action (by server timestamp) wins and both tabs show the same result
- **AC11**: Given two attendees vote on the same concern simultaneously, When votes are submitted, Then both votes are recorded without data loss
- **AC12**: Given the facilitator ends a stage while an attendee submits a question response, When both actions occur within 500ms, Then the response is saved to the ended stage (no data loss)

### Connection Status Visibility

- **AC13**: Given I am connected via SignalR, When viewing the interface, Then I see a subtle "Connected" indicator (green dot)
- **AC14**: Given I am in long polling fallback mode, When viewing the interface, Then I see "Limited connectivity" warning (yellow indicator)
- **AC15**: Given I am disconnected, When viewing the interface, Then I see "Disconnected" error (red indicator) with "Reconnecting..." message
- **AC16**: Given I am a facilitator, When viewing the interface, Then I see "X attendees connected" count updated in real-time via SignalR hub notifications

### Attendee Join/Leave Events

- **AC17**: Given an attendee joins the meeting, When they establish connection, Then the facilitator sees updated attendee count within 2 seconds
- **AC18**: Given an attendee closes their browser, When the connection drops, Then the facilitator sees updated attendee count within 10 seconds (timeout detection)
- **AC19**: Given an attendee rejoins after disconnection, When they reconnect, Then they are counted as the same attendee (session persistence), not a new attendee

## Out of Scope

- Detailed attendee identity tracking (names, avatars)
- Network bandwidth optimization or adaptive quality
- Offline mode with local state persistence
- Peer-to-peer synchronization (all sync goes through server)
- Historical playback of meeting state changes

## Edge Cases & Risks

### Critical Risks

1. **Split-brain scenario**: What if the facilitator's connection drops but they don't realize it?
   - Risk: Facilitator thinks they're controlling meeting but no one sees their actions
   - Mitigation: Aggressive "Disconnected" warning banner, disable all controls when disconnected via Blazor circuit monitoring

2. **Clock skew between clients**: What if attendee's system clock is wrong?
   - Risk: Timers show incorrect remaining time, countdowns desynchronized
   - Mitigation: Use server-provided meeting start timestamp (server-side Blazor handles authoritative time), calculate elapsed time server-side

3. **SignalR scaling limits**: What if 100 attendees join one meeting?
   - Risk: Server overwhelmed, connections dropped
   - Mitigation: Document capacity limits (50 attendees per meeting), Blazor Server circuit limits, load testing required

4. **Browser tab throttling**: What if attendee's browser throttles inactive tabs?
   - Risk: Blazor circuit may disconnect, timers pause when tab is inactive
   - Mitigation: Configure Blazor circuit timeout appropriately, force resync on reactivation via OnAfterRender lifecycle

### Edge Cases

5. **Rapid-fire facilitator actions**: What if facilitator clicks "Start Stage" 10 times in 1 second?
   - Behavior: Debounce actions server-side using MediatR pipeline behaviors (500ms), only process last command, show "Processing..." state

6. **Network jitter causes message reordering**: What if stage transition message arrives before timer start message?
   - Behavior: Server-side command handlers in MediatR ensure transactional consistency; SignalR guarantees message order within a connection

7. **Attendee joins in the middle of a stage**: What if an attendee joins 10 minutes into a 15-minute stage?
   - Behavior: New Blazor circuit receives full state snapshot via OnInitializedAsync, server calculates elapsed time

8. **Facilitator device dies mid-meeting**: What if facilitator's laptop crashes?
   - Behavior: Meeting state persists in PostgreSQL via EF Core, facilitator can reconnect from new device using facilitator link, resume control

9. **Concurrent facilitator link access**: What if someone steals facilitator link and both control meeting?
   - Behavior: Both Blazor circuits see the same state, last command wins (optimistic concurrency in EF Core), no access revocation (security through obscurity)

10. **Zombie connections**: What if Blazor circuit hangs without proper close?
    - Behavior: Blazor Server implements circuit timeout (configurable, default 30s), removes stale connections from count

## UI/UX Requirements

### Connection Status Indicators

1. **Connected State**: Small green dot in top-right corner, tooltip on hover "Connected"
2. **Disconnected State**: Red banner across top "Disconnected from meeting - Reconnecting in Xs", overlay dims main content 20%
3. **Fallback Mode State**: Yellow banner "Limited connectivity - Some features may be delayed"

### Sync Feedback Patterns

1. **Optimistic UI Updates**: When facilitator takes action, update local UI immediately, show subtle "Syncing..." spinner
2. **Confirmation Feedback**: When action confirms from server, remove spinner, no additional message (silent success)
3. **Conflict Resolution Feedback**: If action is rejected (conflict), show toast "Action failed - Another change occurred. Refreshing..."

### Attendee Count Display (Facilitator Only)

- **Location**: Top-right of facilitator interface
- **Format**: "👥 X connected" (e.g., "👥 12 connected")
- **Update Frequency**: Real-time as attendees join/leave
- **Interaction**: Click to see list of anonymized attendee IDs (e.g., "Attendee #1", "Attendee #2") with connection timestamps

## Dependencies

### Internal Features

- **Meeting Creation**: Establishes session identifiers and meeting entities via EF Core
- **All Features**: Every interactive feature depends on reliable sync via SignalR
- **Timer System**: Critically dependent on server-side time (Blazor Server handles authoritative timing)

### External Systems

- **SignalR Hub**: Real-time bi-directional messaging (built into Blazor Server)
- **Long Polling Fallback**: For networks that block WebSockets (automatic SignalR fallback)
- **Server-Side State Store**: Authoritative meeting state in PostgreSQL via EF Core
- **Session Management**: Track connected Blazor circuits, handle reconnections via PostgreSQL session table

## Technical Requirements

### Protocol Specifications

**Technology Stack:**
- **.NET 9** with ASP.NET Core
- **Blazor Server** (no separate API - server-side rendering with SignalR)
- **EF Core 9** with PostgreSQL for persistence
- **MediatR** for CQRS command/query handling
- **Repository Pattern** for data access abstraction

**SignalR Hub Methods:**
```csharp
public interface IMeetingHub
{
    Task StageStarted(StageStartedEvent evt);
    Task ConcernRaised(ConcernRaisedEvent evt);
    Task QuestionAnswered(QuestionAnsweredEvent evt);
    Task MeetingStateChanged(MeetingStateSnapshot snapshot);
}
```

**Event Format (MediatR Notifications):**
```csharp
public record StageStartedEvent(
    string MeetingId,
    string StageId,
    DateTime Timestamp,
    long SequenceNumber
) : INotification;
```

**Blazor Circuit Heartbeat:**
- SignalR automatically maintains connection via ping/pong mechanism
- Blazor Server circuit timeout configurable in `builder.Services.AddServerSideBlazor()`
- Default circuit timeout: 30 seconds of inactivity

**State Snapshot (Blazor Component State):**
```csharp
public class MeetingStateSnapshot
{
    public string MeetingId { get; init; }
    public string? CurrentStageId { get; init; }
    public DateTime? StageStartTimestamp { get; init; }
    public DateTime ServerTimestamp { get; init; }
    public List<ConcernDto> Concerns { get; init; }
    public List<QuestionDto> ActiveQuestions { get; init; }
    public int AttendeeCount { get; init; }
}
```

### Performance Requirements

- **Message Delivery Latency**: 95th percentile < 2 seconds via SignalR
- **Reconnection Time**: Auto-reconnect within 10 seconds of network recovery (Blazor Server automatic reconnection)
- **State Snapshot Size**: < 100KB for typical meeting (50 attendees, 10 stages, 20 concerns)
- **Concurrent Blazor Circuits**: Support 50 concurrent circuits per meeting without degradation

### Reliability Requirements

- **Message Delivery Guarantee**: SignalR provides at-least-once delivery; MediatR command handlers are idempotent
- **Circuit Timeout**: Blazor Server detects dead circuits within 30 seconds (configurable)
- **Fallback Long Polling**: Automatic SignalR fallback when WebSocket unavailable
- **Reconnection Retry**: Blazor Server automatic exponential backoff up to 30 seconds, then constant retries
- **State Persistence**: All meeting state persisted to PostgreSQL via EF Core for recovery after server restart

## Testing Scenarios

### Happy Path

1. 10 attendees join → All see the same agenda → Facilitator starts stage → All see transition within 2s
2. Attendee raises concern → Facilitator sees it immediately → Other attendees see it when they refresh view

### Failure Cases

1. **Network drop simulation**: Disconnect attendee Wi-Fi → See "Disconnected" banner → Reconnect Wi-Fi → Blazor circuit auto-reconnect within 10s
2. **WebSocket blocked**: Load meeting behind corporate firewall → SignalR WebSocket fails → Automatic fallback to long polling activates
3. **Server restart**: Restart .NET server mid-meeting → All Blazor circuits disconnect → Clients reconnect → State recovered from PostgreSQL via EF Core

### Race Conditions

1. **Concurrent stage starts**: Facilitator opens two tabs → Tab A starts Stage 2, Tab B starts Stage 3 simultaneously → Only one stage becomes active (last write wins)
2. **Vote during stage transition**: Attendee votes on concern → Facilitator ends stage at same instant → Vote is saved, no data loss

### Stress Testing

1. 50 attendees join simultaneously → All establish connections within 10 seconds
2. Facilitator rapidly transitions through 10 stages in 30 seconds → All attendees see correct final state
3. 20 attendees raise concerns simultaneously → All concerns appear in facilitator panel within 5 seconds

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-18  
**Status**: Draft - Critical Gap Analysis
