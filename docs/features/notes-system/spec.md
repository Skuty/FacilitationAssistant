# Feature: Notes System

## Problem Statement

Facilitators and attendees need to capture insights, decisions, and action items during meetings without switching to external tools. Notes must support varying visibility levels (public vs private) and be organized by meeting structure (per-stage and overall) while being accessible during and after meetings.

## User Stories

- **US1**: As a facilitator, I want to create notes during meeting setup so that I can prepare talking points before the meeting starts
- **US2**: As a facilitator, I want to mark notes as public or private so that I can control what attendees see
- **US3**: As a facilitator, I want to create notes for specific stages so that context is preserved per agenda item
- **US4**: As an attendee, I want to take my own notes during the meeting so that I can capture personal takeaways
- **US5**: As an attendee, I want to see public facilitator notes so that I understand key points and decisions
- **US6**: As a facilitator, I want to create overall meeting notes (not stage-specific) so that I can capture cross-cutting themes
- **US7**: As a facilitator/attendee, I want to edit my notes during the meeting so that I can refine them as discussion evolves
- **US8**: As a facilitator, I want to see attendee notes (if they choose to share) so that I can understand collective insights
- **US9**: As any participant, I want to access all notes after the meeting via the summary link so that I can review outcomes

## Acceptance Criteria

### Note Creation (Pre-Meeting)

- **AC1**: Given I am a facilitator in meeting setup, When I add a note to a stage, Then I specify: note text (rich text, max 5000 chars), visibility (public/private)
- **AC2**: Given I am a facilitator in meeting setup, When I add a meeting-level note, Then it is not associated with any specific stage
- **AC3**: Given I am creating a note, When I select "Public", Then I see confirmation "This note will be visible to all attendees"
- **AC4**: Given I am creating a note, When I select "Private", Then I see confirmation "Only you will see this note"

### Note Creation (During Meeting)

- **AC5**: Given I am a facilitator and meeting is active, When I view a stage, Then I see "Add Note" button for that stage
- **AC6**: Given I am an attendee and meeting is active, When I view my notes panel, Then I see "Add Note" button
- **AC7**: Given I click "Add Note", When the editor opens, Then I can format text (bold, italic, bullet lists, numbered lists)
- **AC8**: Given I am typing a note, When I save it, Then it appears in the notes panel immediately

### Note Visibility (Facilitator)

- **AC9**: Given I am a facilitator, When I view notes, Then I see: all my public notes, all my private notes, all attendee public notes
- **AC10**: Given a note is private, When I view it, Then it has a "🔒 Private" icon or label
- **AC11**: Given a note is public, When I view it, Then it has a "👁️ Public" icon or label
- **AC12**: Given I view stage-specific notes, When I navigate to that stage, Then notes for that stage are highlighted or filtered

### Note Visibility (Attendee)

- **AC13**: Given I am an attendee, When I view notes, Then I see: all facilitator public notes, all my own notes (public or private), other attendees' public notes (if enabled)
- **AC14**: Given I view facilitator notes, When a note is stage-specific, Then I see which stage it belongs to
- **AC15**: Given I view my notes, When I toggle visibility to "Public", Then other attendees can see it within 2 seconds

### Note Editing

- **AC16**: Given I created a note, When I click "Edit", Then the editor reopens with my previous content
- **AC17**: Given I am editing a note, When I save changes, Then the updated content replaces the original
- **AC18**: Given someone else is viewing my note, When I edit it, Then their view updates in real-time (within 5 seconds)
- **AC19**: Given I am an attendee, When I try to edit a facilitator note, Then I see no edit button (read-only)

### Note Deletion

- **AC20**: Given I created a note, When I click "Delete", Then I see confirmation "Delete this note?"
- **AC21**: Given I confirm deletion, When the note is deleted, Then it is removed from all participants' views within 2 seconds
- **AC22**: Given a note is deleted, When viewing the meeting summary post-meeting, Then the deleted note does not appear

### Stage-Specific Note Organization

- **AC23**: Given notes exist for multiple stages, When I view the notes panel, Then notes are grouped by stage with stage names as headers
- **AC24**: Given a note is meeting-level (not stage-specific), When I view notes, Then it appears in a "General Meeting Notes" section
- **AC25**: Given I am viewing a specific stage, When I filter notes, Then I see only notes for that stage

### Post-Meeting Note Access

- **AC26**: Given the meeting has ended, When I access the meeting summary link, Then I see all public notes organized by stage
- **AC27**: Given I access the facilitator link post-meeting, When I view the summary, Then I see all public and my private notes
- **AC28**: Given I am an attendee accessing post-meeting summary, When I view notes, Then I see facilitator public notes and my own notes

### Rich Text Support

- **AC29**: Given I am creating a note, When I use formatting toolbar, Then I can apply: bold, italic, underline, bullet lists, numbered lists, links
- **AC30**: Given I paste formatted text from another source, When I save, Then basic formatting is preserved (bold, italic, lists)
- **AC31**: Given I am viewing a note with links, When I click a link, Then it opens in a new tab

### Note Timestamps

- **AC32**: Given a note is created, When I view it, Then I see "Created at [time]" timestamp
- **AC33**: Given a note is edited, When I view it, Then I see "Last edited at [time]" timestamp
- **AC34**: Given I am viewing post-meeting summary, When I view notes, Then they are sorted chronologically within each stage

## Out of Scope

- Collaborative real-time editing (multiple people editing same note)
- Note templates or predefined structures
- Note tagging or categorization beyond stage association
- Note export to external formats (PDF, Word) beyond summary page
- Note search functionality
- Attachments or file uploads within notes
- Comments or replies on notes
- Note version history or revision tracking
- Linking notes to specific poll responses or concerns

## Edge Cases & Risks

### Critical Risks

1. **Note content loss**: What if attendee closes browser while typing note?
   - Risk: Unsaved work lost
   - Mitigation: Auto-save draft every 10 seconds to local storage

2. **Simultaneous edit conflicts**: What if facilitator edits note while attendee views it?
   - Risk: Attendee sees stale content
   - Mitigation: Real-time sync, "Last updated 5s ago" indicator

3. **Accidental public note**: What if facilitator marks sensitive note as public?
   - Risk: Confidential info exposed to attendees
   - Mitigation: Confirmation dialog when changing private → public, ability to delete immediately

4. **Note spam**: What if attendee creates 100 notes?
   - Risk: Notes panel unusable
   - Mitigation: Rate limit (20 notes per attendee per meeting), pagination for >50 notes

### Edge Cases

5. **Empty note**: What if user saves a note with no content?
   - Behavior: Validation error "Note cannot be empty", minimum 1 character

6. **Extremely long note**: What if user pastes 10,000-character text?
   - Behavior: Truncate to 5000 chars with validation error, show character count

7. **Note created after stage ends**: What if facilitator adds note to completed stage?
   - Behavior: Allowed, note associated with that stage in summary

8. **Attendee note visibility toggle**: What if attendee changes note from public to private mid-meeting?
   - Behavior: Note immediately hidden from other attendees, facilitator retains access if already viewed (future: fully remove)

9. **Rich text with unsupported tags**: What if user pastes HTML with `<script>` tags?
   - Behavior: Sanitize input, strip dangerous tags, preserve safe formatting

10. **Stage deleted after note created**: What if facilitator removes stage with associated notes (future feature)?
    - Behavior: Notes moved to "General Meeting Notes" section

## UI/UX Requirements

### Notes Panel

1. **Panel Location**:
   - Sidebar or dedicated tab (not blocking agenda)
   - Toggle open/close with "📝 Notes" button
   - Badge showing total note count (e.g., "📝 12")

2. **Notes List Layout**:
   - Grouped by stage: "[Stage 1: Introductions]" header, then notes for that stage
   - Each note card: Author (if visible), timestamp, note content preview (first 100 chars), visibility icon, edit/delete buttons (if owner)
   - Expandable: Click to view full note content

3. **Add Note Button**:
   - Prominent CTA per section: "+ Add Note" in each stage section
   - Click → Opens note editor modal or inline form

### Note Editor

1. **Editor Interface**:
   - Textarea with rich text toolbar (bold, italic, underline, lists, link)
   - Character count "0/5000"
   - Visibility toggle: Radio buttons "Public" / "Private" (default: Private for attendees, Public for facilitator)
   - "Save" and "Cancel" buttons

2. **Auto-Save Indicator**:
   - Show "Saving..." when auto-save occurs (every 10 seconds)
   - Show "Saved" confirmation after successful save

### Facilitator Notes View

1. **Enhanced Display**:
   - Filter dropdown: "All Notes", "Public Only", "Private Only", "My Notes", "Attendee Notes"
   - Visual distinction: Private notes in gray background, public in white

### Post-Meeting Summary

1. **Summary Layout**:
   - Title: "Meeting Summary: [Meeting Name/Date]"
   - Sections: Overall Meeting Progress, Stage-by-Stage Breakdown, Notes by Stage, Poll Results (if any)
   - Each stage section: Stage name, duration, associated notes
   - Print-friendly CSS

### Validation Rules

- **Note Content**: Required, 1-5000 characters (with HTML formatting, count as plain text)
- **Note Visibility**: Must select Public or Private
- **Stage Association**: Optional (can be meeting-level)
- **Rich Text**: Only allow safe HTML tags (b, i, u, ul, ol, li, a, br, p)

### Interaction Patterns

- **Create Note**: Click "+ Add Note" → Modal opens → Type content → Toggle visibility → Click "Save" → Modal closes → Note appears in list
- **Edit Note**: Click "Edit" on note card → Editor reopens with content → Modify → "Save" → Updated content appears
- **Delete Note**: Click "Delete" → Confirmation modal "Are you sure?" → "Yes" → Note removed from list
- **Expand Note**: Click on collapsed note card → Full content expands inline

## Dependencies

### Internal Features

- **Agenda Management**: Stage association depends on stage definitions
- **Real-Time Sync**: Note updates broadcast to all participants
- **Meeting Summary**: Notes aggregated in post-meeting view

### External Systems

- **Rich Text Editor Library**: For formatting toolbar (e.g., Quill, TinyMCE)
- **HTML Sanitization Library**: To prevent XSS attacks
- **Auto-Save Mechanism**: Local storage + server persistence

## Data Model (Conceptual)

```
Note {
  id: uuid
  meeting_id: foreign_key
  stage_id: foreign_key | null (null = meeting-level note)
  author_session_id: string (facilitator or attendee identifier)
  author_type: enum('facilitator', 'attendee')
  content: text (HTML, max 5000 chars plain text equivalent)
  visibility: enum('public', 'private')
  created_at: timestamp
  updated_at: timestamp | null
  deleted_at: timestamp | null (soft delete)
}
```

## Non-Functional Requirements

### Performance

- Note save operation < 1 second
- Real-time note update broadcast < 5 seconds
- Auto-save triggered every 10 seconds (if content changed)

### Reliability

- Note drafts saved to local storage before server submission
- Recovery of unsaved notes on browser refresh (from local storage)
- Note persistence across facilitator/attendee reconnection

### Usability

- Rich text editor keyboard shortcuts (Ctrl+B for bold, etc.)
- Mobile-friendly note editing (large textarea, native keyboard)
- Print-friendly meeting summary (clean formatting, page breaks)

### Security

- HTML sanitization on all note content (prevent XSS)
- Private notes never exposed to unauthorized users (API validation)
- Rate limiting: 20 notes per attendee per meeting

## Testing Scenarios

### Happy Path

1. Facilitator creates public note in Stage 1 setup → Meeting starts → Attendees see note → Facilitator edits note during meeting → Attendees see update
2. Attendee creates private note → Changes to public → Other attendees see note appear

### Complex Flows

1. Facilitator creates 5 public notes across 3 stages → Attendee creates 2 private notes → Post-meeting: Facilitator sees all, attendees see facilitator public + own private
2. Attendee types note → Closes browser → Returns → Sees auto-saved draft → Completes and saves

### Edge Cases

1. Facilitator pastes 10,000-char note → Truncated to 5000 → Validation error shown
2. Attendee saves empty note → "Note cannot be empty" error
3. Facilitator deletes public note mid-meeting → Note removed from all attendees' views within 2 seconds

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Status**: Draft - Pending Review
