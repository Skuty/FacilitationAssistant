# Feature: Meeting Summary & Post-Meeting Access

## Problem Statement

Meeting outcomes, decisions, and data must remain accessible after meetings end. Participants need a single source of truth for notes, poll results, and concerns without requiring active meeting sessions. Facilitators need comprehensive summaries for documentation and follow-up.

## User Stories

- **US1**: As a facilitator, I want to access a comprehensive meeting summary after the meeting ends so that I can review all outcomes
- **US2**: As an attendee, I want to access meeting notes and poll results after the meeting so that I can reference decisions and action items
- **US3**: As a facilitator, I want the summary to include all public and private notes I created so that I have complete context
- **US4**: As an attendee, I want the summary to include my personal notes so that I can review my own takeaways
- **US5**: As any participant, I want the summary to show the final agenda status (completed stages, durations) so that I understand what was covered
- **US6**: As a facilitator, I want the summary to be accessible via the same facilitator link so that I don't need a new URL
- **US7**: As an attendee, I want the summary to be accessible via the same attendee link so that I can return to it later
- **US8**: As a facilitator, I want to download or print the summary so that I can share it with stakeholders
- **US9**: As any participant, I want the summary to include poll results so that I can reference collective decisions
- **US10**: As a facilitator, I want to see which attendees raised concerns and their responses so that I can follow up

## Acceptance Criteria

### Meeting End Transition

- **AC1**: Given a meeting is active, When the facilitator clicks "End Meeting", Then a confirmation dialog appears "End this meeting? This will finalize the summary."
- **AC2**: Given the facilitator confirms ending the meeting, When the meeting ends, Then all active stage timers stop and meeting status changes to "Ended"
- **AC3**: Given the meeting has ended, When anyone accesses either link, Then they are redirected to the read-only summary view
- **AC4**: Given the meeting has ended, When the facilitator tries to control stages or send messages, Then all facilitator controls are hidden/disabled

### Summary Content Structure

- **AC5**: Given a meeting has ended, When I access the summary, Then I see: Meeting title/date, total duration, agenda overview, stage-by-stage breakdown, notes, poll results, concerns summary
- **AC6**: Given the agenda has stages, When I view the summary, Then each stage shows: stage name, planned duration, actual duration, status (completed/skipped), associated notes
- **AC7**: Given stages were completed, When I view the summary, Then completed stages show "Completed in Xm Ys" with green indicator
- **AC8**: Given stages were skipped, When I view the summary, Then skipped stages show "Skipped" with gray indicator
- **AC9**: Given no stages were started, When I view the summary, Then the agenda section shows "Meeting ended without starting stages"

### Notes in Summary (Facilitator View)

- **AC10**: Given I am a facilitator accessing the summary, When I view notes, Then I see: all my public notes, all my private notes, all attendee public notes (if visibility enabled)
- **AC11**: Given notes are associated with stages, When I view the summary, Then notes appear under their respective stage sections
- **AC12**: Given notes are meeting-level (not stage-specific), When I view the summary, Then they appear in a "General Meeting Notes" section
- **AC13**: Given notes have timestamps, When I view the summary, Then each note shows "Created at [time]" or "Edited at [time]"

### Notes in Summary (Attendee View)

- **AC14**: Given I am an attendee accessing the summary, When I view notes, Then I see: all facilitator public notes, all my own notes (public or private)
- **AC15**: Given I created private notes, When I view the summary, Then my private notes are visible only to me (marked with "🔒 Private")
- **AC16**: Given other attendees created public notes, When I view the summary, Then I see them attributed as "Attendee Note" (anonymized)

### Poll Results in Summary

- **AC17**: Given polls were conducted during the meeting, When I view the summary, Then I see a "Poll Results" section listing all questions
- **AC18**: Given a poll had single/multiple choice questions, When I view results, Then I see aggregated percentages and counts per option
- **AC19**: Given a poll had free text responses, When I view results as facilitator, Then I see all text responses (anonymized)
- **AC20**: Given a poll had free text responses, When I view results as attendee, Then I see aggregated responses (if facilitator enabled visibility) or only my own response
- **AC21**: Given a poll was asked multiple times, When I view results, Then each instance is shown separately with timestamps (e.g., "Stage 1 Results", "Stage 3 Results")

### Concerns in Summary

- **AC22**: Given concerns were raised, When I view the summary, Then I see a "Concerns Raised" section listing all concerns
- **AC23**: Given I am a facilitator, When I view concerns in the summary, Then I see: concern text, votes (like/dislike/neutral counts), status (acknowledged/withdrawn), my responses (if provided)
- **AC24**: Given I am an attendee, When I view concerns in the summary, Then I see the same information as facilitator (concerns are public record)
- **AC25**: Given a concern was withdrawn, When I view the summary, Then it is marked "Withdrawn" and visually de-emphasized

### Messages in Summary

- **AC26**: Given messages were sent, When I view the summary, Then I see a "Messages" section listing all facilitator messages
- **AC27**: Given a message was a question with responses, When I view the summary, Then I see aggregated response results (reaction counts, answer percentages, or text responses)
- **AC28**: Given a message was an announcement, When I view the summary, Then I see the message text and timestamp

### Summary Access Persistence

- **AC29**: Given a meeting ended, When I access the facilitator link 7 days later, Then the summary is still accessible
- **AC30**: Given a meeting ended, When I access the attendee link 7 days later, Then the summary is still accessible
- **AC31**: Given a meeting ended more than 30 days ago (future policy), When I access either link, Then I see "This meeting summary has been archived" message

### Summary Formatting & Export

- **AC32**: Given I am viewing the summary, When I click "Print", Then a print-friendly version opens in a new tab (clean formatting, no interactive elements)
- **AC33**: Given I am viewing the summary, When I use browser print function, Then the summary formats correctly across pages (no cut-off sections)
- **AC34**: Given I am viewing the summary, When I want to copy content, Then text is selectable and copyable (not locked)

### Summary Metadata

- **AC35**: Given a meeting has ended, When I view the summary header, Then I see: meeting creation date, meeting start time, meeting end time, total duration
- **AC36**: Given the meeting overran planned duration, When I view the summary, Then I see "Planned: 60m, Actual: 75m (+15m overrun)" in summary header
- **AC37**: Given I am viewing the summary, When I check the page title, Then it shows "Meeting Summary - [Date]" for browser tab identification

### Summary URL Stability

- **AC38**: Given a meeting has ended, When I bookmark the facilitator/attendee link, Then the bookmark remains valid and opens the summary
- **AC39**: Given I share the attendee link post-meeting, When someone clicks it, Then they see the summary (attendee view)

## Out of Scope

- Editing or modifying summary content post-meeting
- Exporting summary to PDF/Word/Excel (beyond browser print)
- Custom summary templates or layouts
- Summary sharing with non-participants (link protection post-meeting)
- Email delivery of summary
- Summary analytics (views, downloads)
- Collaborative post-meeting editing
- Archival or deletion of summaries
- Integration with document management systems

## Edge Cases & Risks

### Critical Risks

1. **Data loss between meeting end and summary generation**: What if server crashes during transition?
   - Risk: Summary incomplete or missing
   - Mitigation: Incremental summary generation during meeting, final snapshot on end

2. **Attendee link shared publicly post-meeting**: What if attendee link goes viral?
   - Risk: Unlimited access to potentially sensitive meeting content
   - Mitigation: (Future) Time-limited links, password protection, or access logs

3. **Summary content too large**: What if meeting had 500 notes and 100 polls?
   - Risk: Summary page slow to load, unreadable
   - Mitigation: Pagination or collapsible sections for large datasets

4. **Facilitator private notes exposed**: What if bug shows private notes to attendees?
   - Risk: Confidential information leaked
   - Mitigation: Strict access control validation, API-level filtering

### Edge Cases

5. **Meeting ended without starting**: What if facilitator creates meeting but never starts it?
   - Behavior: Summary shows "Meeting created but not started. No stages were run."

6. **Meeting ended mid-stage**: What if facilitator ends meeting while Stage 2 is active?
   - Behavior: Stage 2 marked as "Incomplete" with actual time shown, remaining stages "Not Started"

7. **No notes or polls created**: What if meeting had only agenda and timer?
   - Behavior: Summary shows "No notes recorded" and "No polls conducted" placeholders

8. **Attendee accesses summary immediately after meeting end**: What if summary generation takes 10 seconds?
   - Behavior: Show "Generating summary..." loading state, auto-refresh when ready

9. **Print summary with very long notes**: What if a note is 5000 characters?
   - Behavior: Allow text to flow across pages naturally, avoid orphaned headings

10. **Summary accessed on mobile**: What if user views summary on small screen?
    - Behavior: Responsive layout, collapsible sections, scrollable tables

## UI/UX Requirements

### Summary Page Layout

1. **Header Section**:
   - Meeting title: "Meeting Summary - [Date]"
   - Metadata: Created [date], Started [time], Ended [time], Duration [Xh Ym]
   - Status indicator: "Meeting Completed" (green)
   - Action buttons: "Print", "Copy Link"

2. **Navigation (if summary is long)**:
   - Sticky table of contents: "Jump to: Agenda | Notes | Polls | Concerns | Messages"
   - Click to scroll to section

3. **Agenda Overview Section**:
   - Table or timeline view of all stages
   - Columns: Stage Name, Planned Duration, Actual Duration, Status
   - Visual indicators: ✓ Completed (green), ⏭️ Skipped (gray), ⏸️ Incomplete (yellow)

4. **Stage-by-Stage Breakdown**:
   - Each stage as a collapsible card
   - Header: Stage name, duration, status
   - Content: Associated notes, polls triggered at this stage

5. **Notes Section**:
   - Grouped by stage, then meeting-level
   - Each note card: Author type (Facilitator/Attendee), timestamp, content, visibility icon
   - Facilitator notes in one style, attendee notes in another

6. **Poll Results Section**:
   - Each poll as a card: Question text, response count, results visualization
   - Charts: Bar charts for choice questions, list for free text

7. **Concerns Section**:
   - Table or list of concerns
   - Columns: Concern text, Votes, Status, Facilitator response

8. **Messages Section**:
   - List of messages with timestamps
   - Show responses if question-type messages

### Print-Friendly Formatting

1. **Print CSS**:
   - Remove: Navigation, action buttons, interactive elements
   - Preserve: All text content, headings, timestamps
   - Page breaks: Avoid breaking stage sections across pages
   - Font: Readable size (12pt minimum)

### Mobile Responsiveness

1. **Small Screen Layout**:
   - Single-column layout
   - Collapsible sections (tap to expand)
   - Horizontal scrolling for tables if necessary
   - Large touch targets for expand/collapse

### Validation Rules

- None (summary is read-only, no input validation needed)

### Interaction Patterns

- **View Summary**: Access link → See header → Scroll through sections → Click to expand collapsed content
- **Print Summary**: Click "Print" → Print dialog opens → Select printer or "Save as PDF"
- **Copy Link**: Click "Copy Link" → "Link copied!" confirmation → Can paste in email/chat

## Dependencies

### Internal Features

- **Meeting Creation**: Meeting end status determined by meeting lifecycle
- **Agenda Management**: Stage completion status feeds into summary
- **Notes System**: All notes aggregated in summary
- **Polling System**: Poll results displayed in summary
- **Concerns System**: Concerns and responses included in summary
- **Messaging System**: Messages and responses included in summary

### External Systems

- **Summary Generation Service**: Aggregates all meeting data into structured summary
- **Print Rendering**: Browser print API or server-side PDF generation (future)

## Data Model (Conceptual)

```
MeetingSummary {
  meeting_id: foreign_key (primary key)
  generated_at: timestamp
  total_duration_seconds: integer
  stages_completed: integer
  stages_skipped: integer
  notes_count: integer
  polls_count: integer
  concerns_count: integer
  messages_count: integer
  summary_json: jsonb (cached aggregated data for fast display)
}
```

## Non-Functional Requirements

### Performance

- Summary page load time < 3 seconds for meetings with <100 combined entities (notes, polls, concerns)
- Summary generation on meeting end < 5 seconds
- Print rendering < 10 seconds

### Reliability

- Summary data persists indefinitely (or per retention policy, e.g., 30 days)
- Summary accessible even if facilitator/attendees disconnect
- Summary generation idempotent (can regenerate if needed)

### Usability

- Summary readable without prior context (includes all necessary labels)
- Print output professional and shareable
- Mobile summary usable without zooming

### Security

- Attendee link shows only permitted data (no private facilitator notes)
- Summary links maintain same access control as during meeting
- No modification of summary content post-meeting

## Testing Scenarios

### Happy Path

1. Facilitator ends meeting → Summary generates → Access facilitator link → See all notes (public + private), poll results, concerns
2. Attendee accesses summary link → See facilitator public notes, own private notes, poll results (if visible), concerns

### Complex Flows

1. Meeting with 10 stages, 50 notes, 20 polls → Summary generates correctly → All data organized by stage → Print works cleanly
2. Meeting ended mid-stage → Summary shows Stage 2 as "Incomplete (ran 15m of 20m planned)"

### Edge Cases

1. Meeting created but never started → Summary shows "Meeting not started"
2. Meeting with zero notes/polls → Summary shows "No notes recorded" placeholders
3. Attendee accesses summary on mobile → Responsive layout displays correctly

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Status**: Draft - Pending Review
