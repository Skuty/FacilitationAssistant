# Feature: Attendee Identity & Session Management

## Problem Statement

The application allows attendees to join without authentication, but this creates ambiguity around identity, session persistence, and data ownership. When an attendee reconnects, raises concerns, answers questions, or creates notes, the system must reliably associate actions with the same attendee across disconnections without requiring login.

## User Stories

- **US1**: As an attendee, I want my session to persist across page refreshes so that I don't lose my question responses and notes
- **US2**: As an attendee, I want to optionally provide a display name so that my notes and concerns are attributed to me
- **US3**: As a facilitator, I want to distinguish between unique attendees and duplicate connections so that I can accurately count participation
- **US4**: As an attendee, I want my concerns and notes to be editable/deletable only by me so that others can't modify my contributions
- **US5**: As a facilitator, I want to see anonymized attendee identifiers so that I can reference specific attendees without exposing their identity
- **US6**: As an attendee, I want my session data (responses, notes) to be deleted after the meeting ends so that my data isn't retained unnecessarily

## Acceptance Criteria

### Session Creation & Persistence

- **AC1**: Given I access an attendee link for the first time, When the page loads, Then a unique session identifier is generated and stored in browser localStorage
- **AC2**: Given I have an existing session, When I refresh the page, Then my session identifier is retrieved from localStorage and I reconnect as the same attendee
- **AC3**: Given I clear browser data or use incognito mode, When I access the attendee link again, Then I am treated as a new attendee with a new session ID
- **AC4**: Given I join from a mobile device and later join from desktop, When I access from different devices, Then I am counted as two separate attendees (no cross-device session sync)

### Optional Display Name

- **AC5**: Given I join as an attendee, When the interface loads, Then I see an optional "Set Display Name" field (max 30 characters)
- **AC6**: Given I set a display name, When I submit it, Then it is stored in my session and appears on my notes and concerns
- **AC7**: Given I don't set a display name, When I create notes or concerns, Then they are attributed to "Attendee #X" where X is my auto-generated number (e.g., "Attendee #5")
- **AC8**: Given I set a display name, When I change it mid-meeting, Then all my existing contributions are retroactively updated to show the new name
- **AC9**: Given two attendees choose the same display name, When viewing contributions, Then both names are suffixed with session ID (e.g., "John (A5)", "John (A7)")

### Attendee Numbering & Identity

- **AC10**: Given attendees join the meeting, When they connect, Then they are assigned sequential numbers in join order (Attendee #1, Attendee #2, etc.)
- **AC11**: Given I am Attendee #5, When I disconnect and reconnect using the same session, Then I retain the same number (no renumbering)
- **AC12**: Given Attendee #3 leaves permanently, When new attendees join, Then the number #3 is not reused (gaps are preserved)
- **AC13**: Given I am viewing the facilitator interface, When I see attendee-generated content, Then I see the display name or "Attendee #X" label

### Data Ownership & Permissions

- **AC14**: Given I raised a concern, When I view it in the concerns panel, Then I see "Edit" and "Delete" buttons (only visible on my own concerns)
- **AC15**: Given another attendee raised a concern, When I view it, Then I cannot edit or delete it (buttons not visible)
- **AC16**: Given I created a note, When I view it in the notes panel, Then I can edit or delete it (ownership based on session ID)
- **AC17**: Given I answered a question, When the question is closed, Then I cannot change my answer (immutable after question closure)

### Session Expiration & Data Retention

- **AC18**: Given a meeting has ended, When 48 hours pass, Then all attendee session data (responses, notes) is deleted from the server
- **AC19**: Given a meeting is active, When I don't interact for 8 hours, Then my session remains valid (no inactivity timeout during active meetings)
- **AC20**: Given a meeting ended 7 days ago, When I access the attendee link, Then I see read-only summary but cannot create new content (session no longer active)

## Out of Scope

- Email-based identity verification
- Social login (Google, Facebook, etc.)
- Cross-meeting identity persistence (each meeting is isolated)
- Attendee profiles or avatars
- Facilitator ability to ban or remove specific attendees
- Attendee-to-attendee private messaging
- Export of individual attendee data (GDPR compliance assumed simple due to minimal data)

## Edge Cases & Risks

### Critical Risks

1. **Session ID collision**: What if two attendees generate the same session ID?
   - Risk: Data mixing, one attendee's actions attributed to another
   - Mitigation: Use UUIDv4 (collision probability negligible), validate uniqueness on server

2. **Display name spoofing**: What if an attendee impersonates the facilitator by setting name "Facilitator"?
   - Risk: Confusion, attendees think notes/concerns are from facilitator
   - Mitigation: Reserve "Facilitator" as a blocked display name, show role badge on facilitator content

3. **Session hijacking via link sharing**: What if an attendee shares their sessionStorage with another person?
   - Risk: Multiple people control the same "attendee" identity
   - Mitigation: Document risk (out of scope for v1), no technical prevention

4. **Browser fingerprinting leakage**: What if session IDs reveal user identity through side channels?
   - Risk: Anonymity compromised
   - Mitigation: Use cryptographically random UUIDs, no browser fingerprinting

### Edge Cases

5. **Attendee joins before facilitator starts meeting**: What if an attendee accesses the link during meeting setup?
   - Behavior: Show "Meeting starting soon" message, session is created but inactive, activates when meeting starts

6. **Attendee #1 leaves, then rejoins as Attendee #50**: What if first joiner reconnects after 49 others?
   - Behavior: Retains original number (#1), no renumbering

7. **Display name with special characters**: What if attendee enters name "Attendee <script>alert('XSS')</script>"?
   - Behavior: Sanitize display name (strip HTML tags, encode special characters), show "Invalid characters removed" warning

8. **Incognito mode attendee**: What if attendee joins in incognito, then joins in normal browser?
   - Behavior: Two separate sessions, counted as two different attendees (expected behavior)

9. **Session storage full**: What if browser localStorage is full or disabled?
   - Behavior: Fallback to sessionStorage (session lost on tab close), show warning "Session may not persist across page refreshes"

10. **Concurrent browser tabs with same link**: What if attendee opens attendee link in two tabs?
    - Behavior: Both tabs share the same session ID (read from localStorage), actions from either tab attributed to same attendee

## UI/UX Requirements

### Display Name Setup

1. **Location**: Top of attendee interface, dismissible after first set
2. **Prompt**: "Set your display name (optional)" with text input field
3. **Validation**: 1-30 characters, no HTML tags, alphanumeric + spaces + basic punctuation only
4. **Save Button**: "Save Name" → On success, show "Display name saved ✓" for 2 seconds
5. **Edit Option**: Once set, show "Editing as: [Name]" with small edit icon to change

### Attendee Identifier Display

1. **Facilitator View**: All attendee content shows "Display Name (A#)" or "Attendee #X"
2. **Attendee View**: Own content shows "You", others' content shows their display name or "Attendee #X"
3. **Color Coding**: Each attendee number has consistent color across notes/concerns (e.g., Attendee #1 always blue)

### Session Status Indicators

1. **Session Active**: No indicator needed (silent success)
2. **Session Warning**: If localStorage unavailable, show banner "Your session may not persist across page refreshes"
3. **Session Lost**: If session ID missing on reconnect, show "New session created - Previous responses may not be visible"

## Dependencies

### Internal Features

- **Real-Time Sync**: Session ID used to authenticate WebSocket connections
- **Concerns System**: Ownership based on session ID
- **Notes System**: Ownership based on session ID
- **Polling System**: Response attribution based on session ID
- **Meeting Summary**: Session data aggregated for post-meeting view

### External Systems

- **UUID Generation Library**: Client-side UUID v4 generator
- **Browser Storage API**: localStorage for session persistence
- **Server-Side Session Store**: Map session IDs to attendee numbers and display names
- **Content Sanitization**: Prevent XSS in display names

## Data Model (Conceptual)

```
AttendeeSession {
  session_id: uuid (primary key)
  meeting_id: foreign_key
  attendee_number: integer (auto-increment per meeting)
  display_name: string(30) | null
  first_seen_at: timestamp
  last_seen_at: timestamp
}

ContentOwnership {
  content_id: uuid (concern_id, note_id, etc.)
  content_type: 'concern' | 'note' | 'response'
  session_id: foreign_key
}
```

## Non-Functional Requirements

### Privacy

- Session IDs must be cryptographically random (128-bit entropy)
- Display names are publicly visible to all meeting participants (no privacy guarantee)
- No tracking of attendee behavior across multiple meetings (GDPR-friendly)

### Performance

- Session validation on each WebSocket message < 10ms
- Display name update propagates to all clients within 2 seconds
- Session lookup in attendee count query < 50ms

### Usability

- Display name prompt is non-intrusive (collapsible, dismissible)
- Session persistence works without user awareness (transparent)
- Clear attribution of user-generated content (no confusion about authorship)

## Testing Scenarios

### Happy Path

1. Attendee joins → Session created → Sets display name "Alice" → Creates note → Note shows "Alice"
2. Alice refreshes page → Session persists → Previous note still attributed to "Alice"
3. Alice disconnects → Rejoins next day → Session persists if meeting still active

### Failure Cases

1. **localStorage disabled**: Attendee joins → Warning shown → Session survives page refresh (sessionStorage fallback)
2. **Session ID collision** (simulated): Two attendees with same UUID → Server rejects second, forces regeneration
3. **XSS attempt**: Attendee sets name "<script>alert(1)</script>" → Sanitized to "scriptalert1script"

### Edge Cases

1. Two attendees set name "John" → Both shown as "John (A3)" and "John (A7)" in facilitator view
2. Attendee joins in incognito → Creates note → Closes browser → Reopens incognito link → Previous note not visible (expected)
3. Attendee opens 5 tabs with same link → All tabs share session → Actions from any tab attributed to same attendee

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-18  
**Status**: Draft - Critical Gap Analysis
