# Feature: Concerns & Feedback System

## Problem Statement

Attendees need non-disruptive ways to raise concerns or signal confusion during meetings without verbally interrupting. Facilitators need real-time awareness of attendee concerns to adapt meeting flow, while attendees need peer validation mechanisms to gauge shared sentiment.

## User Stories

- **US1**: As an attendee, I want to raise a concern using predefined options so that I can quickly signal an issue without typing
- **US2**: As an attendee, I want to raise a custom concern with free text so that I can describe specific issues not covered by fixed options
- **US3**: As a facilitator, I want to see attendee concerns in real-time so that I can address issues before they escalate
- **US4**: As an attendee, I want to vote on other attendees' concerns so that I can show agreement or disagreement
- **US5**: As a facilitator, I want to acknowledge concerns so that attendees know I've seen their feedback
- **US6**: As a facilitator, I want to respond to concerns with text so that I can provide clarification or action items
- **US7**: As an attendee, I want to see how many others share my concern so that I feel validated or understand if it's unique to me
- **US8**: As a facilitator, I want concerns to not pause the meeting timer so that feedback collection doesn't disrupt time management
- **US9**: As an attendee, I want to withdraw a concern I raised so that I can remove it if the issue was resolved

## Acceptance Criteria

### Raising Concerns (Attendee)

- **AC1**: Given I am an attendee, When I click "Raise Concern", Then I see a modal with predefined options: "Meeting Overrunning", "Topic Unclear", "Technical Issue", "Need Break", "Custom Reason"
- **AC2**: Given I select a predefined concern, When I click "Submit", Then the concern is sent to the facilitator and visible to other attendees within 2 seconds
- **AC3**: Given I select "Custom Reason", When I submit, Then I must provide text (10-200 characters) before submission is enabled
- **AC4**: Given I raise a concern, When it is submitted, Then I see confirmation "Concern raised" and the concern appears in the concerns panel
- **AC5**: Given I have already raised a concern, When I try to raise another concern, Then I can submit it (no limit on number of concerns per attendee)

### Concerns Display (All Participants)

- **AC6**: Given concerns exist, When I view the interface, Then I see a concerns panel showing: concern text, timestamp, vote counts (like/dislike/neutral), status (new/acknowledged)
- **AC7**: Given multiple concerns are raised, When I view the concerns panel, Then they are sorted by most recent first
- **AC8**: Given a concern has votes, When I view it, Then I see vote breakdown: X likes, Y dislikes, Z neutral
- **AC9**: Given I am viewing a concern I raised, When it appears in the panel, Then it is visually marked as "Your Concern"

### Voting on Concerns (Attendee)

- **AC10**: Given a concern is visible, When I click "Like" (thumbs up), Then my vote is registered and count increments by 1
- **AC11**: Given a concern is visible, When I click "Dislike" (thumbs down), Then my vote is registered and count increments by 1
- **AC12**: Given a concern is visible, When I click "Neutral" (neutral emoji), Then my vote is registered and count increments by 1
- **AC13**: Given I have voted on a concern, When I click a different vote option, Then my previous vote is replaced (only one vote per attendee per concern)
- **AC14**: Given I have voted on a concern, When I click the same vote option again, Then my vote is removed (toggle off)
- **AC15**: Given votes are cast, When I view vote counts, Then they update in real-time (within 2 seconds of vote submission)

### Facilitator Concern Management

- **AC16**: Given concerns are raised, When I view the facilitator interface, Then I see a dedicated concerns panel with real-time updates
- **AC17**: Given a new concern appears, When I view the concerns panel, Then unacknowledged concerns are highlighted (bold, different color)
- **AC18**: Given I am viewing a concern, When I click "Acknowledge", Then the concern status changes to "Acknowledged" and highlight is removed
- **AC19**: Given I acknowledge a concern, When attendees view it, Then they see "Acknowledged by Facilitator" label with timestamp
- **AC20**: Given I want to respond to a concern, When I click "Respond", Then I can type a response (max 500 characters) that is visible to all attendees

### Facilitator Responses

- **AC21**: Given I have responded to a concern, When attendees view the concern, Then they see my response text below the original concern
- **AC22**: Given I want to edit my response, When I click "Edit Response", Then I can modify the text and re-submit
- **AC23**: Given I want to remove my response, When I click "Delete Response", Then the response is removed (concern remains with "Acknowledged" status)

### Concern Withdrawal (Attendee)

- **AC24**: Given I raised a concern, When I click "Withdraw" on my concern, Then it is marked as "Withdrawn" and grayed out
- **AC25**: Given a concern is withdrawn, When others view it, Then they can still see the original text and votes but cannot vote anymore
- **AC26**: Given a concern is withdrawn, When the facilitator views it, Then it is visually de-emphasized (moved to bottom or collapsed section)

### Real-Time Behavior

- **AC27**: Given a concern is raised, When the facilitator is viewing the interface, Then they see a notification badge or alert indicating new concern count
- **AC28**: Given the facilitator acknowledges a concern, When attendees are viewing it, Then the status updates in real-time without page refresh
- **AC29**: Given concerns do not affect meeting timer, When I raise a concern during a stage, Then the stage timer continues counting

### Concerns Panel Visibility

- **AC30**: Given I am an attendee, When I view the meeting interface, Then the concerns panel is accessible via a tab or sidebar (not blocking agenda)
- **AC31**: Given the concerns panel is open, When I close it, Then it collapses but shows a badge with active concern count
- **AC32**: Given no concerns have been raised, When I view the concerns panel, Then it shows "No concerns raised yet"

## Out of Scope

- Private concerns (visible only to facilitator)
- Concern categories or tagging
- Concern resolution workflows (marking as "Resolved")
- Historical concern trends or analytics
- Notifications outside the meeting interface (email, push)
- Concern priority levels or escalation
- Anonymous vs identified concerns (v1.0 all concerns are anonymized to "Attendee X")

## Edge Cases & Risks

### Critical Risks

1. **Concern spam**: What if an attendee raises 20 concerns in 1 minute?
   - Risk: Concerns panel unusable, facilitator overwhelmed
   - Mitigation: Rate limit (max 5 concerns per attendee per 5 minutes), show warning "Slow down, please"

2. **Offensive custom concern text**: What if attendee writes inappropriate content?
   - Risk: Other attendees see offensive text, facilitator distracted
   - Mitigation: (Future) Content moderation, facilitator can hide concern

3. **Vote manipulation**: What if one attendee votes on all concerns to inflate numbers?
   - Risk: Misleading vote counts, concerns appear more/less important
   - Mitigation: Show total voters, not just vote count (e.g., "5 votes from 3 attendees")

4. **Facilitator ignores concerns**: What if facilitator never acknowledges any concerns?
   - Risk: Attendees feel unheard, disengage
   - Mitigation: (Future) Highlight concerns with >5 likes, auto-nudge facilitator

### Edge Cases

5. **Zero votes on concern**: What if no one votes on a concern?
   - Behavior: Show "0 votes" or hide vote count, concern still visible

6. **Concern raised after meeting ends**: What if attendee tries to raise concern post-meeting?
   - Behavior: "Raise Concern" button disabled, message "Meeting has ended"

7. **Facilitator responds to withdrawn concern**: What if facilitator responds to a concern that was then withdrawn?
   - Behavior: Response persists, both concern and response visible (grayed out)

8. **Extremely long custom concern**: What if attendee pastes 2000-character text?
   - Behavior: Truncate to 200 chars with validation error, show character count

9. **Attendee withdraws concern with 50 votes**: What if highly-voted concern is withdrawn?
   - Behavior: Votes preserved, concern moved to "Withdrawn" section, facilitator notified

10. **Facilitator acknowledges all concerns at once**: What if facilitator bulk-acknowledges 30 concerns?
    - Behavior: Support bulk action (future), v1.0 requires individual acknowledgment

## UI/UX Requirements

### Concerns Panel (Attendee)

1. **Panel Location**:
   - Sidebar or bottom panel (does not block agenda)
   - Toggle open/close with icon button (e.g., "🚩 Concerns")
   - Badge showing count of unacknowledged concerns (e.g., "🚩 3")

2. **Concern Card Layout**:
   - Header: Concern text, timestamp (e.g., "2 minutes ago")
   - Voting buttons: 👍 Like (X), 👎 Dislike (Y), 😐 Neutral (Z)
   - Footer: Status label ("New", "Acknowledged", "Withdrawn"), optional facilitator response
   - If user's concern: "Your Concern" badge

3. **Raise Concern Button**:
   - Prominent CTA: "Raise Concern" or "🚩 Raise Issue"
   - Opens modal with predefined options as large buttons + "Custom" option

### Concerns Panel (Facilitator)

1. **Enhanced Display**:
   - Same layout as attendee, plus:
   - Action buttons: "Acknowledge", "Respond"
   - Unacknowledged concerns highlighted (bold, red border, or "NEW" badge)
   - Notification badge on concerns icon for new concerns

2. **Response Interface**:
   - Click "Respond" → Inline textarea appears below concern
   - Character count "0/500"
   - "Send" and "Cancel" buttons
   - Sent responses editable/deletable

### Voting Interaction

1. **Vote Buttons**:
   - Icon-based: 👍 👎 😐 (universal symbols)
   - Show count next to each icon (e.g., "👍 5")
   - Active state: Button highlighted if user voted this option
   - Hover state: Show tooltip "Like this concern"

2. **Vote Animation**:
   - Click → Button scales briefly → Count increments
   - If toggling off: Count decrements smoothly

### Validation Rules

- **Custom Concern Text**: Required if "Custom" selected, 10-200 characters
- **Facilitator Response**: 1-500 characters, no HTML allowed
- **Rate Limiting**: Max 5 concerns per attendee per 5 minutes

### Interaction Patterns

- **Raise Concern**: Click button → Modal opens → Select option → Confirm → Modal closes → See concern in panel
- **Vote on Concern**: Click vote button → Immediate visual feedback → Count updates
- **Acknowledge Concern (Facilitator)**: Click "Acknowledge" → No confirmation → Status updates → Badge removed
- **Respond to Concern (Facilitator)**: Click "Respond" → Textarea appears → Type → Click "Send" → Response visible to all

## Dependencies

### Internal Features

- **Real-Time Sync**: WebSocket for broadcasting concerns and votes
- **Agenda Management**: Concerns do not affect stage timer, but may reference current stage context
- **Messaging System**: Facilitator responses are similar to broadcast messages

### External Systems

- **Rate Limiting Service**: To prevent concern spam
- **WebSocket/Polling**: For real-time updates to facilitator and attendees

## Data Model (Conceptual)

```
Concern {
  id: uuid
  meeting_id: foreign_key
  stage_id: foreign_key | null (stage active when raised)
  attendee_session_id: string (anonymous identifier)
  concern_type: enum('meeting_overrunning', 'topic_unclear', 'technical_issue', 'need_break', 'custom')
  concern_text: string(200) (required for custom, null for predefined)
  status: enum('new', 'acknowledged', 'withdrawn')
  raised_at: timestamp
  acknowledged_at: timestamp | null
  withdrawn_at: timestamp | null
}

ConcernVote {
  id: uuid
  concern_id: foreign_key
  attendee_session_id: string
  vote_type: enum('like', 'dislike', 'neutral')
  voted_at: timestamp
}

ConcernResponse {
  id: uuid
  concern_id: foreign_key
  response_text: string(500)
  responded_at: timestamp
  updated_at: timestamp | null
}
```

## Non-Functional Requirements

### Performance

- Concern submission and broadcast < 2 seconds
- Vote registration and count update < 1 second
- Facilitator notification on new concern < 2 seconds

### Reliability

- Concerns persist across network disconnection
- Vote deduplication (one vote per attendee per concern)
- Concern state recoverable if facilitator reconnects

### Usability

- Concerns panel scrollable if >10 concerns exist
- Mobile-friendly voting buttons (48x48px touch targets)
- Clear visual hierarchy (unacknowledged concerns most prominent)

### Security

- Rate limiting on concern submission (5 per 5 minutes per attendee)
- Profanity filter on custom concern text (future enhancement)
- Facilitator cannot see individual attendee identifiers (anonymized)

## Testing Scenarios

### Happy Path

1. Attendee raises "Meeting Overrunning" concern → Facilitator sees notification → Acknowledges → Attendee sees "Acknowledged" status
2. Attendee raises custom concern "Audio quality poor" → Other attendees vote 👍 (5 likes) → Facilitator responds "Checking audio settings" → All see response

### Complex Flows

1. Attendee raises concern → 10 attendees vote → Facilitator acknowledges → Attendee withdraws concern → Votes preserved, concern grayed out
2. Multiple attendees raise similar concerns → Facilitator bulk-responds (future) → All see same response

### Edge Cases

1. Attendee raises 5 concerns in 2 minutes → 6th concern blocked → "Please wait 3 minutes" message
2. Facilitator acknowledges concern, then attendee withdraws → Both statuses coexist (acknowledged + withdrawn)
3. No concerns raised entire meeting → Panel shows "No concerns yet, meeting going smoothly"

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Status**: Draft - Pending Review
