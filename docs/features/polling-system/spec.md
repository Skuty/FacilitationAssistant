# Feature: Questions & Polling System

## Problem Statement

Facilitators need structured ways to gather attendee input without disrupting meeting flow. Attendees need low-friction mechanisms to provide feedback, opinions, and answers without verbally interrupting, while facilitators need control over when questions are asked and who sees results.

## User Stories

- **US1**: As a facilitator, I want to define questions during meeting setup so that I can plan data collection points
- **US2**: As a facilitator, I want to associate questions with specific stages so that questions appear automatically when stages start
- **US3**: As a facilitator, I want to add ad-hoc questions during the meeting so that I can respond to emerging needs
- **US4**: As a facilitator, I want to choose from a question template library so that I don't retype common questions
- **US5**: As an attendee, I want to see questions relevant to the current stage so that I can provide timely input
- **US6**: As an attendee, I want to answer with predefined options or free text so that I can respond quickly or elaborately
- **US7**: As a facilitator, I want to control whether attendees see aggregated results so that I can manage transparency
- **US8**: As a facilitator, I want to ask the same question multiple times so that I can track sentiment changes
- **US9**: As an attendee, I want to see which questions I've already answered so that I don't submit duplicates unintentionally
- **US10**: As a facilitator, I want to see real-time response counts so that I know when enough people have answered

## Acceptance Criteria

### Question Definition (Pre-Meeting)

- **AC1**: Given I am a facilitator in meeting setup, When I add a question, Then I specify: question text (max 300 chars), answer type (single choice, multiple choice, free text, scale), trigger timing (meeting start, stage start, manual)
- **AC2**: Given I am defining a question, When I select single/multiple choice, Then I can add 2-10 answer options (each max 100 chars)
- **AC3**: Given I am defining a question, When I select scale answer type, Then I specify scale range (e.g., 1-5, 1-10) and optional labels (e.g., "Strongly Disagree" to "Strongly Agree")
- **AC4**: Given I am adding a question, When I associate it with a stage, Then I can set it to auto-show when stage starts or remain manual-trigger only
- **AC5**: Given I am defining a question, When I configure result visibility, Then I choose: "Facilitator Only", "Show Aggregated Results to Attendees", or "Show All Individual Responses to Attendees"

### Question Template Library

- **AC6**: Given I am adding a question, When I click "Use Template", Then I see a library of common questions (e.g., "Are you there?", "Rate this stage", "Any blockers?")
- **AC7**: Given I am viewing the template library, When I select a template, Then it pre-fills question text and answer options, which I can edit
- **AC8**: Given I have created custom questions before (future enhancement), When I access templates, Then I also see my recently used questions

### Ad-Hoc Questions During Meeting

- **AC9**: Given I am a facilitator and meeting is active, When I click "Add Question", Then I can create a new question with same options as pre-meeting setup
- **AC10**: Given I am a facilitator, When I add an ad-hoc question, Then I choose to show it immediately or save for later manual trigger
- **AC11**: Given I added an ad-hoc question, When I mark it as "Show Immediately", Then all attendees see it within 2 seconds

### Attendee Question Display

- **AC12**: Given a question is triggered for attendees, When I view the interface, Then the question appears as a modal/panel overlay requiring interaction before dismissal
- **AC13**: Given multiple questions are active simultaneously, When I view the interface, Then questions are queued and shown one at a time (FIFO)
- **AC14**: Given a question is displayed to me, When I submit an answer, Then the question dismisses and I see confirmation "Answer submitted"
- **AC15**: Given a question is displayed to me, When I click "Skip", Then the question dismisses without submitting (facilitator sees who skipped)

### Answer Submission

- **AC16**: Given a single-choice question is displayed, When I select an option, Then only one option can be selected at a time
- **AC17**: Given a multiple-choice question is displayed, When I select options, Then I can select up to the defined maximum (e.g., "Select up to 3")
- **AC18**: Given a free-text question is displayed, When I type my answer, Then I can enter up to 1000 characters with visible character count
- **AC19**: Given a scale question is displayed, When I interact with it, Then I see a slider or radio buttons for the scale range
- **AC20**: Given I submit an answer, When the submission completes, Then I cannot change my answer unless facilitator allows re-submission

### Facilitator Results View

- **AC21**: Given attendees have answered a question, When I view results as facilitator, Then I see: total responses, total skips, total pending (not yet answered)
- **AC22**: Given a single-choice question has responses, When I view results, Then I see bar chart or percentage breakdown per option
- **AC23**: Given a free-text question has responses, When I view results, Then I see a list of all submitted text answers (anonymized or with attendee identifiers based on settings)
- **AC24**: Given a scale question has responses, When I view results, Then I see average score, distribution histogram, and individual values
- **AC25**: Given I am viewing results, When responses come in real-time, Then the display updates every 5 seconds without full page refresh

### Attendee Results View (Conditional)

- **AC26**: Given a question is configured as "Show Aggregated Results to Attendees", When I submit my answer, Then I see the same aggregated view as facilitator (no individual identification)
- **AC27**: Given a question is configured as "Facilitator Only", When I submit my answer, Then I see only confirmation "Answer submitted", no results
- **AC28**: Given a question is configured as "Show All Individual Responses", When I submit my answer, Then I see a list of all answers (anonymized or with labels like "Attendee 1, 2, 3")

### Repeated Questions

- **AC29**: Given a question was asked at Stage 1, When the facilitator asks the same question again at Stage 3, Then it is treated as a new instance (separate responses)
- **AC30**: Given I answered a question in Stage 1, When the same question is asked again in Stage 3, Then I can answer again independently
- **AC31**: Given a question is asked multiple times, When the facilitator views results, Then results are segmented by instance/timing (e.g., "Stage 1 Results" vs "Stage 3 Results")

### Question Status Tracking

- **AC32**: Given a question was triggered, When I view my attendee interface, Then I see a list of "Answered" and "Pending" questions in a sidebar or dedicated section
- **AC33**: Given I have pending questions, When I navigate away from the meeting view, Then I see a badge indicating "X questions pending"
- **AC34**: Given a question expires (facilitator closes it), When I view my pending questions, Then it is removed from the list (can no longer answer)

## Out of Scope

- Ranked choice voting or advanced poll types
- Anonymous vs identified responses (v1.0 assumes all anonymous)
- Question branching (conditional questions based on previous answers)
- Exporting poll results to CSV/Excel
- Question response deadlines or time limits
- Mandatory questions (cannot skip)
- Real-time results visualization (live charts updating as people vote)

## Edge Cases & Risks

### Critical Risks

1. **Response submission race condition**: What if attendee submits answer but network fails?
   - Risk: Answer lost, attendee thinks it submitted
   - Mitigation: Retry logic with exponential backoff, show "Submitting..." state

2. **Facilitator doesn't see responses**: What if real-time update fails?
   - Risk: Facilitator thinks no one answered, asks again
   - Mitigation: Manual refresh button, periodic polling fallback

3. **Too many simultaneous questions**: What if facilitator triggers 5 questions at once?
   - Risk: Attendee overwhelmed, skips all
   - Mitigation: Queue questions, show "3 more questions pending" indicator

4. **Free-text abuse**: What if attendee submits offensive content?
   - Risk: Other attendees see inappropriate text (if results shared)
   - Mitigation: (Future) Content moderation, facilitator can hide responses

### Edge Cases

5. **Zero responses**: What if no one answers a question?
   - Behavior: Facilitator sees "0 responses, 15 skipped, 10 pending"

6. **Attendee joins mid-meeting**: What if attendee arrives after questions were asked?
   - Behavior: Can answer still-active questions, cannot answer closed questions

7. **Question text too long**: What if facilitator pastes 2000-character question?
   - Behavior: Truncate to 300 chars with validation error, show character count

8. **Template library empty**: What if no templates are defined yet?
   - Behavior: Show "No templates available" message, allow manual question creation

9. **Attendee leaves before answering**: What if attendee closes browser with pending questions?
   - Behavior: Counted as "pending" until meeting ends, then "not answered"

10. **Scale question with invalid range**: What if facilitator sets scale 5-5 (same min/max)?
    - Behavior: Validation error, minimum range of 2 points required

## UI/UX Requirements

### Question Display (Attendee)

1. **Question Modal/Panel**:
   - Appears as centered modal (desktop) or bottom sheet (mobile)
   - Contains: Question text, answer options, "Submit" button, "Skip" link
   - Cannot be dismissed without action (submit or skip)

2. **Answer Options Layout**:
   - Single/multiple choice: Radio buttons or checkboxes, vertically stacked
   - Free text: Textarea with character count "0/1000"
   - Scale: Horizontal slider with labeled endpoints or clickable number buttons

3. **Pending Questions Indicator**:
   - Badge on question icon/tab showing "3 pending"
   - Clicking opens questions panel showing list of unanswered questions

### Facilitator Question Management

1. **Question Builder**:
   - Inline form: Question text input, answer type dropdown, trigger options
   - "Add Option" button for choice-based questions
   - Toggle for "Show results to attendees"

2. **Question List View**:
   - Table or cards showing: Question text, Type, Trigger, Responses (X/Y answered)
   - Actions: View Results, Close Question, Ask Again
   - Filter by status: Active, Closed, Scheduled

3. **Results Display**:
   - Choice questions: Bar chart with percentages
   - Free text: Scrollable list with search/filter
   - Scale: Average score (e.g., "3.8/5") + histogram
   - Real-time update indicator (e.g., "Last updated 5s ago")

### Validation Rules

- **Question Text**: Required, 10-300 characters
- **Answer Options**: Minimum 2, maximum 10 per question
- **Option Text**: 1-100 characters each
- **Free Text Answer**: 0-1000 characters
- **Scale Range**: Minimum 2 points (e.g., 1-2), maximum 10 points (e.g., 1-10)
- **Trigger Timing**: Must select at least one (meeting start, stage start, or manual)

### Interaction Patterns

- **Submit Answer**: Click "Submit" → Show spinner → "Answer submitted" confirmation → Auto-dismiss modal
- **Skip Question**: Click "Skip" → No confirmation, immediate dismiss
- **View Pending Questions**: Click badge → Opens sidebar with question list → Click question to answer
- **Close Question (Facilitator)**: Click "Close" → Confirmation "Stop accepting answers?" → Question locked

## Dependencies

### Internal Features

- **Agenda Management**: Stage-based question triggers depend on stage lifecycle
- **Real-Time Sync**: Question display and result updates require WebSocket
- **Notes System**: Question results may be referenced in notes

### External Systems

- **Question Template Repository**: Database of pre-defined question templates
- **Real-Time Broadcast**: WebSocket for pushing questions to attendees

## Data Model (Conceptual)

```
Question {
  id: uuid
  meeting_id: foreign_key
  text: string(300)
  answer_type: enum('single_choice', 'multiple_choice', 'free_text', 'scale')
  trigger_type: enum('meeting_start', 'stage_start', 'manual')
  associated_stage_id: uuid | null
  result_visibility: enum('facilitator_only', 'aggregated', 'all_responses')
  status: enum('draft', 'active', 'closed')
  created_at: timestamp
  triggered_at: timestamp | null
}

QuestionOption {
  id: uuid
  question_id: foreign_key
  option_text: string(100)
  order_index: integer
}

QuestionResponse {
  id: uuid
  question_id: foreign_key
  attendee_session_id: string (anonymous identifier)
  answer_choice_ids: array<uuid> | null (for choice questions)
  answer_text: string(1000) | null (for free text)
  answer_scale_value: integer | null (for scale)
  submitted_at: timestamp
  status: enum('submitted', 'skipped')
}
```

## Non-Functional Requirements

### Performance

- Question display to attendees < 2 seconds after trigger
- Response submission confirmation < 1 second
- Results aggregation update < 5 seconds

### Reliability

- Response submission retries up to 3 times on network failure
- Offline responses queued and submitted when connection restores
- Question state persists across facilitator reconnection

### Usability

- Question modal does not block critical meeting information (agenda still visible)
- Mobile-friendly input methods (large touch targets, native keyboard)
- Screen reader compatible (ARIA labels on all inputs)

## Testing Scenarios

### Happy Path

1. Facilitator adds question to Stage 2 with auto-trigger → Stage 2 starts → Attendees see question → Submit answers → Facilitator sees results
2. Facilitator adds ad-hoc free-text question → Triggers immediately → Attendees type answers → Facilitator reads responses

### Complex Flows

1. Attendee answers 3 questions → Joins Stage 4 late → Sees 2 pending questions → Answers → All responses recorded
2. Facilitator asks "Rate this stage" at Stage 1 → Asks same question at Stage 3 → Both sets of responses are separate

### Edge Cases

1. No attendees answer question → Facilitator sees "0 responses, 10 skipped"
2. Attendee submits answer, network fails → Retries → Answer eventually submitted
3. Facilitator closes question while attendee is typing → Attendee sees "Question closed" message, cannot submit

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Status**: Draft - Pending Review
