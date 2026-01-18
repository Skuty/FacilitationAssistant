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

- **AC1**: Given I access a meeting link, When the page loads, Then a WebSocket connection is established within 3 seconds or fallback to polling occurs
- **AC2**: Given WebSocket connection fails, When fallback to polling activates, Then I see a warning indicator "Limited connectivity - using fallback mode"
- **AC3**: Given I lose network connection, When connection is lost, Then I see "Disconnected - Reconnecting..." message with retry countdown
- **AC4**: Given I was disconnected, When network returns, Then reconnection attempts occur at: 1s, 2s, 5s, 10s, 30s intervals (exponential backoff)
- **AC5**: Given I reconnect successfully, When connection is reestablished, Then I receive full current meeting state snapshot to resync

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

- **AC13**: Given I am connected via WebSocket, When viewing the interface, Then I see a subtle "Connected" indicator (green dot)
- **AC14**: Given I am in polling fallback mode, When viewing the interface, Then I see "Limited connectivity" warning (yellow indicator)
- **AC15**: Given I am disconnected, When viewing the interface, Then I see "Disconnected" error (red indicator) with "Reconnecting..." message
- **AC16**: Given I am a facilitator, When viewing the interface, Then I see "X attendees connected" count updated in real-time

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
   - Mitigation: Aggressive "Disconnected" warning banner, disable all controls when disconnected

2. **Clock skew between clients**: What if attendee's system clock is wrong?
   - Risk: Timers show incorrect remaining time, countdowns desynchronized
   - Mitigation: Use server-provided meeting start timestamp, calculate elapsed time client-side relative to server time

3. **WebSocket scaling limits**: What if 100 attendees join one meeting?
   - Risk: Server overwhelmed, connections dropped
   - Mitigation: Document capacity limits (50 attendees per meeting), load testing required

4. **Browser tab throttling**: What if attendee's browser throttles inactive tabs?
   - Risk: Timers pause when tab is inactive, attendee rejoins to find meeting far ahead
   - Mitigation: Use Page Visibility API to detect tab switches, force resync on reactivation

### Edge Cases

5. **Rapid-fire facilitator actions**: What if facilitator clicks "Start Stage" 10 times in 1 second?
   - Behavior: Debounce actions client-side (500ms), only send last action, show "Processing..." state

6. **Network jitter causes message reordering**: What if stage transition message arrives before timer start message?
   - Behavior: Server assigns sequence numbers to all messages, clients apply in order regardless of arrival time

7. **Attendee joins in the middle of a stage**: What if an attendee joins 10 minutes into a 15-minute stage?
   - Behavior: New attendee receives full state snapshot including stage start timestamp, calculates elapsed time correctly

8. **Facilitator device dies mid-meeting**: What if facilitator's laptop crashes?
   - Behavior: Meeting state persists on server, facilitator can reconnect from new device using facilitator link, resume control

9. **Concurrent facilitator link access**: What if someone steals facilitator link and both control meeting?
   - Behavior: Both see the same state, last action wins, no access revocation (security through obscurity)

10. **Zombie connections**: What if client connection hangs without proper close?
    - Behavior: Server implements connection timeout (30s of no heartbeat), removes stale connections from count

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

- **Meeting Creation**: Establishes session identifiers for sync channels
- **All Features**: Every interactive feature depends on reliable sync
- **Timer System**: Critically dependent on synchronized clocks

### External Systems

- **WebSocket Server**: Push-based real-time messaging
- **HTTP Polling Fallback**: For networks that block WebSockets
- **Server-Side State Store**: Authoritative meeting state (Redis or equivalent)
- **Session Management**: Track connected clients, handle reconnections

## Technical Requirements

### Protocol Specifications

**WebSocket Message Format:**
```json
{
  "type": "stage_started" | "concern_raised" | "question_answered" | ...,
  "timestamp": 1705601234567,
  "sequence_number": 42,
  "meeting_id": "abc123",
  "payload": { ... }
}
```

**Heartbeat Mechanism:**
- Client sends heartbeat ping every 15 seconds
- Server responds with pong + current server timestamp
- Detect clock drift by comparing round-trip time and timestamp delta

**State Snapshot Format:**
```json
{
  "meeting_id": "abc123",
  "current_stage_id": "stage_5",
  "stage_start_timestamp": 1705601234567,
  "server_timestamp": 1705601500000,
  "concerns": [...],
  "active_questions": [...],
  "attendee_count": 12
}
```

### Performance Requirements

- **Message Delivery Latency**: 95th percentile < 2 seconds
- **Reconnection Time**: Auto-reconnect within 10 seconds of network recovery
- **State Snapshot Size**: < 100KB for typical meeting (50 attendees, 10 stages, 20 concerns)
- **Concurrent Connections**: Support 50 clients per meeting without degradation

### Reliability Requirements

- **Message Delivery Guarantee**: At-least-once delivery (idempotent handlers to handle duplicates)
- **Connection Timeout**: Detect dead connections within 30 seconds
- **Fallback Polling Rate**: 2-second interval when WebSocket unavailable
- **Reconnection Retry**: Exponential backoff up to 30 seconds, then constant 30-second retries

## Testing Scenarios

### Happy Path

1. 10 attendees join → All see the same agenda → Facilitator starts stage → All see transition within 2s
2. Attendee raises concern → Facilitator sees it immediately → Other attendees see it when they refresh view

### Failure Cases

1. **Network drop simulation**: Disconnect attendee Wi-Fi → See "Disconnected" banner → Reconnect Wi-Fi → Auto-reconnect within 10s
2. **WebSocket blocked**: Load meeting behind corporate firewall → WebSocket fails → Fallback to polling activates
3. **Server restart**: Restart server mid-meeting → All clients disconnect → Clients reconnect → State persists from database

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
