# Feature: Messaging & Communication System

## Problem Statement

Facilitators need to broadcast important information to all attendees during meetings without relying on verbal announcements. Attendees need low-friction ways to respond to facilitator messages without disrupting meeting flow. Traditional chat systems create noise and distraction, while this system should support structured broadcast-response patterns.

## User Stories

- **US1**: As a facilitator, I want to broadcast messages to all attendees so that I can share links, announcements, or instructions
- **US2**: As an attendee, I want to see facilitator messages as notifications so that important information is not missed
- **US3**: As an attendee, I want to scroll back through message history so that I can review previous announcements
- **US4**: As an attendee, I want to respond to messages with reactions so that I can acknowledge without typing
- **US5**: As an attendee, I want to respond to messages with predefined answers so that I can provide quick feedback
- **US6**: As an attendee, I want to respond to messages with free text so that I can provide detailed input
- **US7**: As a facilitator, I want to see all message responses so that I understand attendee reactions and feedback
- **US8**: As a facilitator, I want to distinguish between announcements and questions so that I can manage expectations for responses
- **US9**: As an attendee, I want messages to not block the main interface so that I can continue viewing the agenda

## Acceptance Criteria

### Message Creation (Facilitator)

- **AC1**: Given I am a facilitator, When I click "Send Message", Then I see a message composer with: text input (max 1000 chars), message type (Announcement/Question), optional response options
- **AC2**: Given I am composing a message, When I select "Announcement", Then no response mechanism is attached
- **AC3**: Given I am composing a message, When I select "Question", Then I can define: response type (reactions only, predefined answers, free text)
- **AC4**: Given I select "Predefined Answers", When I define options, Then I can add 2-6 answer options (each max 100 chars)
- **AC5**: Given I complete the message, When I click "Send", Then all attendees receive it within 2 seconds

### Message Display (Attendee - Immediate View)

- **AC6**: Given a facilitator sends a message, When I am viewing the interface, Then the full message immediately appears in a dialog or prominent overlay (no click required)
- **AC7**: Given the message is displayed, When I interact with the response options or click dismiss, Then the message overlay closes
- **AC8**: Given a message is displayed, When I do not interact with it, Then it remains visible until I dismiss it (no auto-dismiss for active messages)
- **AC9**: Given multiple messages are sent quickly, When I view messages, Then they queue and display one at a time (FIFO) - the next one appears after I close the current one

### Message Display (Full View)

- **AC10**: Given I open a message, When I view it, Then I see: message text, timestamp, message type label (Announcement/Question), response options (if question type)
- **AC11**: Given a message is an announcement, When I view it, Then I see only "Dismiss" button
- **AC12**: Given a message is a question with reactions, When I view it, Then I see reaction buttons (👍 👎 ❤️ 😂 😮)
- **AC13**: Given a message is a question with predefined answers, When I view it, Then I see answer options as clickable buttons
- **AC14**: Given a message is a question with free text, When I view it, Then I see a textarea (max 500 chars) and "Submit" button

### Message History

- **AC15**: Given messages have been sent, When I click "Messages" or "📩 Messages" icon, Then I see a scrollable list of all messages in chronological order (newest first or oldest first based on UX)
- **AC16**: Given I view message history, When I scroll through messages, Then I see: message text, timestamp, response count (if question), my response (if I responded)
- **AC17**: Given I view message history, When I click on a message, Then it expands to show full details and response options (if not yet responded)
- **AC18**: Given I have already responded to a message, When I view it in history, Then I see "You responded: [my response]" and cannot change response

### Attendee Responses - Reactions

- **AC19**: Given a message has reaction options, When I click a reaction, Then my reaction is registered and count increments
- **AC20**: Given I have reacted, When I click a different reaction, Then my previous reaction is replaced
- **AC21**: Given I have reacted, When I click the same reaction again, Then my reaction is removed (toggle off)
- **AC22**: Given reactions are submitted, When I view the message, Then I see reaction counts (e.g., "👍 5, ❤️ 3")

### Attendee Responses - Predefined Answers

- **AC23**: Given a message has predefined answer options, When I click an option, Then my answer is submitted and I see "Response recorded"
- **AC24**: Given I have answered, When I view the message again, Then I see "You answered: [option text]" and cannot change answer (unless facilitator allows)
- **AC25**: Given I submit an answer, When other attendees are viewing aggregated results (if enabled), Then they see updated counts within 5 seconds

### Attendee Responses - Free Text

- **AC26**: Given a message has free text response, When I type and submit, Then my text (max 500 chars) is sent to facilitator
- **AC27**: Given I submit free text, When I view the message again, Then I see "You responded: [my text]"
- **AC28**: Given I have submitted free text, When I want to edit, Then I cannot (no editing post-submission)

### Facilitator Response View

- **AC29**: Given attendees have responded to a message, When I view it as facilitator, Then I see: total responses, response breakdown (reactions counts, answer option counts, or list of free text responses)
- **AC30**: Given a message has reactions, When I view results, Then I see "👍 5 (25%), ❤️ 3 (15%)" with percentages
- **AC31**: Given a message has free text responses, When I view results, Then I see a list of all text responses (anonymized as "Attendee 1, 2, 3...")
- **AC32**: Given no one has responded, When I view the message, Then I see "0 responses" with option to resend or close message

### Message Lifecycle

- **AC33**: Given a message is sent, When the facilitator views it, Then they can "Close Message" to stop accepting responses
- **AC34**: Given a message is closed, When attendees view it, Then response options are disabled with message "Responses closed"
- **AC35**: Given a message is closed, When attendees view it in history, Then it shows final response counts/results (if results visible to attendees)

### Real-Time Behavior

- **AC36**: Given a facilitator sends a message, When attendees are viewing the interface, Then the notification appears within 2 seconds
- **AC37**: Given an attendee responds, When the facilitator is viewing the message, Then the response count updates within 5 seconds
- **AC38**: Given multiple attendees respond simultaneously, When I view results, Then all responses are captured without loss

## Out of Scope

- Direct messaging between attendees
- Threaded replies or conversation chains
- Message editing or deletion after sending
- Rich media attachments (images, files, videos)
- Message priority levels or pinning
- Message read receipts (who has seen the message)
- Message search or filtering
- Message templates or saved drafts
- Integration with external messaging platforms

## Edge Cases & Risks

### Critical Risks

1. **Notification spam**: What if facilitator sends 10 messages in 1 minute?
   - Risk: Attendees overwhelmed, miss important info
   - Mitigation: Rate limit (5 messages per 5 minutes), show "Queue: 3 messages pending"

2. **Message response loss**: What if attendee submits response but network fails?
   - Risk: Response not recorded, attendee thinks it was
   - Mitigation: Retry logic, show "Submitting..." state, confirmation only after server ack

3. **Free text abuse**: What if attendee submits offensive content?
   - Risk: Facilitator sees inappropriate text
   - Mitigation: (Future) Content moderation, facilitator can flag/hide responses

4. **Message timing confusion**: What if message sent during stage transition?
   - Risk: Attendees miss notification during UI update
   - Mitigation: Queue messages during transitions, show after transition completes

### Edge Cases

5. **Empty message**: What if facilitator sends message with no text?
   - Behavior: Validation error "Message cannot be empty", minimum 5 characters

6. **Extremely long message**: What if facilitator pastes 5000-character message?
   - Behavior: Truncate to 1000 chars with validation error, show character count

7. **Zero responses**: What if no one responds to a question message?
   - Behavior: Facilitator sees "0 responses" with option to resend as reminder

8. **Attendee joins mid-meeting**: What if attendee arrives after messages sent?
   - Behavior: Can view message history, can respond to still-open messages

9. **Message sent after meeting ends**: What if facilitator tries to send message post-meeting?
   - Behavior: "Send Message" button disabled, message "Meeting has ended"

10. **Duplicate responses**: What if attendee clicks submit button twice rapidly?
    - Behavior: Debounce submission, only first response recorded, show "Already submitted"

## UI/UX Requirements

### Message Composer (Facilitator)

1. **Composer Interface**:
   - Click "Send Message" button → Opens modal or side panel
   - Textarea for message (1000 char limit, show count)
   - Radio buttons: "Announcement" / "Question"
   - If "Question": Dropdown for response type (Reactions, Predefined Answers, Free Text)
   - If "Predefined Answers": Input fields for options (2-6)
   - "Send" and "Cancel" buttons

### Notification / Overlay (Attendee)

1.  **Immediate Display**:
   - Appears immediately as a centered modal or bottom-sheet (mobile)
   - Non-blocking backdrop (user can still see timer/agenda behind it, but focus is on message)
   - Format: Header "New Message", Full Text, Response Options (if any), Dismiss/Close button
   - If multiple messages: Show "1 of 3" indicator

2.  **Styling**:
   - Distinct elevation/shadow to separate from content
   - Animation: Pop/Fade in
   - Close button (X) top right

### Message History Panel

1. **Panel Layout**:
   - Accessible via "📩 Messages" button in toolbar
   - Scrollable list of messages (reverse chronological or chronological)
   - Each message card: Icon (📢 Announcement or ❓ Question), text preview, timestamp, response status

2. **Message Card**:
   - Click to expand inline or open modal
   - If responded: Show "✓ Responded" badge
   - If closed: Show "🔒 Closed" badge

### Response Interface (Attendee)

1. **Reactions**:
   - Horizontal row of emoji buttons (👍 👎 ❤️ 😂 😮)
   - Show count next to each (e.g., "👍 5")
   - Highlight selected reaction

2. **Predefined Answers**:
   - Large buttons (one per option) stacked vertically
   - Selected answer shows checkmark, others disabled after selection

3. **Free Text**:
   - Textarea (full width, 3-4 rows)
   - Character count "0/500"
   - "Submit" button below textarea

### Facilitator Results View

1. **Results Display**:
   - Reactions: Horizontal bar chart or emoji counts
   - Predefined Answers: Percentage breakdown (e.g., "Yes: 60% (12), No: 40% (8)")
   - Free Text: List of responses (scrollable, anonymized)
   - "Close Message" button to stop accepting responses

### Validation Rules

- **Message Text**: Required, 5-1000 characters
- **Predefined Answer Options**: 2-6 options, each 1-100 characters
- **Free Text Response**: 1-500 characters
- **Rate Limiting**: 5 messages per facilitator per 5 minutes

### Interaction Patterns

- **Send Message**: Compose → Click "Send" → Modal closes → All attendees see notification within 2s
- **Respond to Message**: Notification appears → Click "View" → Modal opens → Select response → Click submit → "Response recorded" → Modal closes
- **View Message History**: Click "📩" → Panel opens → Scroll through messages → Click message to expand

## Dependencies

### Internal Features

- **Real-Time Sync**: WebSocket for broadcasting messages and responses
- **Polling System**: Message questions similar to poll questions (may share response logic)
- **Concerns System**: Responses to messages similar to concern voting patterns

### External Systems

- **WebSocket/Polling**: For real-time message delivery
- **Rate Limiting Service**: To prevent message spam

## Data Model (Conceptual)

```
Message {
  id: uuid
  meeting_id: foreign_key
  message_text: string(1000)
  message_type: enum('announcement', 'question')
  response_type: enum('none', 'reactions', 'predefined', 'free_text') | null
  status: enum('active', 'closed')
  sent_at: timestamp
  closed_at: timestamp | null
}

MessageOption {
  id: uuid
  message_id: foreign_key
  option_text: string(100)
  order_index: integer
}

MessageResponse {
  id: uuid
  message_id: foreign_key
  attendee_session_id: string
  response_type: enum('reaction', 'predefined', 'free_text')
  reaction_emoji: string | null (e.g., '👍')
  selected_option_id: uuid | null
  free_text_response: string(500) | null
  responded_at: timestamp
}
```

## Non-Functional Requirements

### Performance

- Message broadcast to all attendees < 2 seconds
- Response submission confirmation < 1 second
- Results aggregation update < 5 seconds

### Reliability

- Message delivery guaranteed (retry on failure)
- Response deduplication (one response per attendee per message)
- Message state persists across facilitator/attendee reconnection

### Usability

- Notifications do not block critical UI (agenda, timer)
- Mobile-friendly response buttons (48x48px touch targets)
- Clear visual distinction between announcements and questions

### Security

- Rate limiting: 5 messages per facilitator per 5 minutes
- HTML sanitization on message text and free text responses
- Anonymized response attribution (no personal identifiers exposed)

## Testing Scenarios

### Happy Path

1. Facilitator sends announcement "Take a 5-minute break" → Attendees see notification → Click "View" → See message → Click "Dismiss"
2. Facilitator sends question with reactions → Attendees respond with 👍 → Facilitator sees "👍 15 (75%)"

### Complex Flows

1. Facilitator sends 3 messages rapidly → Attendees see notifications queue → View one at a time → Respond to each
2. Attendee joins mid-meeting → Opens message history → Sees 5 past messages → Responds to 2 still-open questions

### Edge Cases

1. Facilitator sends message with 1000 characters → Attendees see full message in modal
2. No attendees respond to question → Facilitator sees "0 responses" → Closes message
3. Attendee submits response, network fails → Retry succeeds → "Response recorded" shown

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Status**: Draft - Pending Review
