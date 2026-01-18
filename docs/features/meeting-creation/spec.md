# Feature: Meeting Creation & Link Management

## Problem Statement

Users need to create meetings instantly without authentication while maintaining security through unique, role-based access links. Facilitators require exclusive control capabilities that attendees must not access.

## User Stories

- **US1**: As a facilitator, I want to create a meeting without signing up so that I can start facilitating immediately
- **US2**: As a facilitator, I want two separate links (facilitator and attendee) so that I can control the meeting while attendees have read-only access
- **US3**: As a facilitator, I want to define agenda stages with time allocations before sharing the attendee link so that attendees see a structured meeting from the start
- **US4**: As an attendee, I want to join a meeting by clicking a link so that I don't need to create an account or remember credentials
- **US5**: As a facilitator, I want to reuse my facilitator link from different devices so that I can recover from device failure
- **US6**: As a facilitator, I want to copy both links easily so that I can share them via email, chat, or calendar invites

## Acceptance Criteria

### Meeting Creation

- **AC1**: Given I am on the homepage, When I click "Create Meeting", Then a new meeting is generated with two unique links (facilitator and attendee)
- **AC2**: Given a meeting is created, When I access the facilitator link, Then I see the meeting setup interface with agenda builder
- **AC3**: Given a meeting is created, When I access the attendee link before agenda is published, Then I see "Meeting not started yet" message
- **AC4**: Given meeting links are generated, When they are displayed, Then each link is copyable with one-click copy button
- **AC5**: Given I create a meeting, When the meeting is generated, Then both links contain unique, non-sequential, URL-safe identifiers (minimum 16 characters)

### Link Security

- **AC6**: Given I have an attendee link, When I try to access facilitator-only functions (stage transitions, agenda editing), Then these controls are not visible and API requests return 403 Forbidden
- **AC7**: Given I have a facilitator link, When I access it from a different browser/device, Then I retain full facilitator permissions
- **AC8**: Given a meeting exists, When someone tries to guess link patterns (sequential IDs, common words), Then they cannot access the meeting (randomized identifiers)

### Agenda Setup (Pre-Meeting)

- **AC9**: Given I am a facilitator in setup mode, When I add agenda stages, Then each stage requires: name (max 100 chars), planned duration (1-240 minutes), optional description (max 500 chars)
- **AC10**: Given I have added agenda stages, When I reorder them via drag-and-drop, Then the stage order updates without data loss
- **AC11**: Given I am building an agenda, When I set total meeting duration, Then it is independent of the sum of stage durations (stages can exceed total time)
- **AC12**: Given I have defined the agenda, When I click "Start Meeting", Then the attendee link becomes active and shows the agenda
- **AC13**: Given I have started the meeting, When I access the facilitator link again, Then I cannot edit the agenda structure (stage names/order), only manage live meeting

### Link Persistence

- **AC14**: Given I have created a meeting, When I close the browser and return to the facilitator link within 48 hours, Then the meeting state persists
- **AC15**: Given a meeting has ended, When I access either link after 7 days, Then I see a read-only summary (not full interactive meeting)

## Out of Scope

- Meeting templates or saved agendas for reuse
- Authentication or user accounts
- Password protection for links
- Expiring links with time-based access control
- Editing meeting title or metadata after creation
- Transferring facilitator permissions to another user
- Deleting or canceling meetings
- Custom branding or meeting themes

## Edge Cases & Risks

### Critical Risks

1. **Link collision**: What if two meetings generate the same ID?
   - Risk: Attendee joins wrong meeting, data leakage
   - Mitigation: Use cryptographically strong random IDs (UUID v4 or equivalent)

2. **Facilitator link leakage**: What if facilitator accidentally shares facilitator link publicly?
   - Risk: Unauthorized control of meeting
   - Mitigation: Visual distinction (color, icon, warning label), confirmation dialog on share

3. **Meeting state loss**: What if server restarts during meeting?
   - Risk: All participants disconnected, agenda lost
   - Mitigation: Persistent storage with write-ahead logging

4. **Browser back button during setup**: What if facilitator hits back after creating meeting?
   - Risk: Creates duplicate meeting, loses original links
   - Mitigation: Store last-created meeting in sessionStorage, show "Resume setup" option

### Edge Cases

5. **Empty agenda**: What if facilitator starts meeting without adding stages?
   - Behavior: Prevent "Start Meeting" button until at least 1 stage added, show validation message

6. **Zero-duration stages**: What if facilitator sets stage duration to 0?
   - Behavior: Minimum 1 minute duration enforced, validation error on save

7. **Extremely long stage names**: What if facilitator pastes 1000-character stage name?
   - Behavior: Truncate to 100 chars with visual indicator, show character count

8. **Concurrent facilitator access**: What if facilitator opens facilitator link in two browser tabs?
   - Behavior: Both tabs show real-time sync, last action wins (optimistic locking)

9. **Attendee accesses link during setup**: What if attendee link is shared before meeting starts?
   - Behavior: Show "Meeting starting soon" message, auto-refresh when meeting goes live

10. **Mobile link sharing**: What if facilitator uses mobile to create meeting?
    - Behavior: Native share dialog for links, fallback to clipboard copy

## UI/UX Requirements

### Meeting Creation Flow

1. **Homepage**:
   - Single prominent CTA: "Create New Meeting"
   - Optional: Show sample meeting screenshot/demo
   - No navigation complexity

2. **Meeting Created Confirmation**:
   - Display both links in separate, clearly labeled boxes
   - Visual distinction: Facilitator link in primary color, Attendee link in secondary color
   - Each link has: Label, URL, Copy button, QR code (for mobile joining)
   - Warning: "Do not share your Facilitator link publicly"

3. **Agenda Builder**:
   - Inline stage creation (click "Add Stage" shows form)
   - Drag handles visible on each stage for reordering
   - Real-time character count on name/description fields
   - Overall meeting duration input (separate from stage durations)
   - "Start Meeting" button disabled until ≥1 stage added

### Validation Rules

- **Stage Name**: Required, 1-100 characters, no special characters that break URLs
- **Stage Duration**: Required, integer 1-240 minutes
- **Stage Description**: Optional, 0-500 characters
- **Overall Meeting Duration**: Optional, integer 1-480 minutes (if provided)
- **Minimum Stages**: At least 1 stage required to start meeting

### Interaction Patterns

- **Copy Link Button**: Click → "Copied!" feedback for 2 seconds → return to "Copy" label
- **Stage Reordering**: Drag-and-drop with visual placeholder showing drop zone
- **Start Meeting**: Click → Confirmation dialog "Ready to start? Attendees will be able to join." → Go live
- **Link Display**: Auto-select on click for easy manual copy
- **Browser Close Warning**: Show "Are you sure? Meeting links will be lost if not saved" if links not copied

## Dependencies

### Internal Features

- **Agenda Management**: Meeting creation provides initial agenda structure
- **Real-Time Sync**: Link access determines permission level for WebSocket connection
- **Meeting Summary**: Link lifecycle determines when meeting transitions to read-only mode

### External Systems

- **Unique ID Generation**: Cryptographically secure random string generator
- **Persistent Storage**: Database for meeting metadata and state
- **URL Routing**: Map link IDs to meeting sessions

## Data Model (Conceptual)

```
Meeting {
  id: unique_id (primary key)
  facilitator_token: unique_id (secret)
  attendee_token: unique_id (public)
  created_at: timestamp
  started_at: timestamp | null
  ended_at: timestamp | null
  status: 'setup' | 'active' | 'ended'
  total_duration_minutes: integer | null
}

AgendaStage {
  id: unique_id (primary key)
  meeting_id: foreign_key
  name: string(100)
  description: string(500)
  planned_duration_minutes: integer
  order_index: integer
}
```

## Non-Functional Requirements

### Security

- Facilitator and attendee tokens must be cryptographically random (128-bit entropy minimum)
- Links must not be enumerable (no sequential patterns)
- Rate limiting on meeting creation (10 per IP per hour)

### Performance

- Meeting creation response time < 500ms
- Link copy action feedback < 100ms
- Agenda save operation < 1 second

### Usability

- Links must be valid URLs (no special encoding required for sharing)
- Copy button must work on iOS Safari, Android Chrome, desktop browsers
- QR codes must be scannable from 1 meter distance on standard displays

## Testing Scenarios

### Happy Path

1. Create meeting → See both links → Copy facilitator link → Open in new tab → See agenda builder
2. Add 3 stages → Set durations → Start meeting → Open attendee link → See agenda live

### Failure Cases

1. Create meeting → Close browser immediately → Cannot recover links (expected behavior)
2. Share facilitator link by mistake → Unauthorized user controls meeting (document risk)
3. Network interruption during creation → Retry mechanism shows last state or starts fresh

### Boundary Testing

1. Create meeting with 1 stage (minimum)
2. Create meeting with 50 stages (stress test)
3. Set stage duration to 240 minutes (maximum)
4. Use 100-character stage name (maximum)

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Status**: Draft - Pending Review
