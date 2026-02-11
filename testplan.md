# Facilitation Assistant - E2E Test Plan
**Version**: 1.0  
**Last Updated**: February 11, 2026  
**Testing Framework**: Playwright for .NET + xUnit  
**Target**: Blazor Server Application

---

## 1. Overview

### 1.1 Purpose
This document outlines the end-to-end (E2E) testing strategy for the Facilitation Assistant application using Playwright for .NET with xUnit. Tests will validate critical user journeys across all features from the perspective of both facilitators and attendees.

### 1.2 Test Environment
- **Framework**: Microsoft.Playwright + xUnit
- **Browser Targets**: Chromium (primary), Firefox, WebKit (Safari)
- **Application**: Blazor Server with SignalR
- **Database**: PostgreSQL (test instance) / In-Memory for isolated tests
- **Test Data**: Generated via factories, cleaned up after each test class

### 1.3 Test Architecture
```
tests/
  FacilitationAssistant.E2ETests/
    ├── Fixtures/
    │   ├── PlaywrightFixture.cs         # Browser setup/teardown
    │   ├── WebApplicationFixture.cs     # ASP.NET app hosting
    │   └── TestDataFactory.cs           # Test data generation
    ├── PageObjects/
    │   ├── HomePage.cs
    │   ├── FacilitatorPage.cs
    │   ├── AttendeePage.cs
    │   ├── SummaryPage.cs
    │   └── Components/
    │       ├── AgendaPanel.cs
    │       ├── NotesPanel.cs
    │       ├── ConcernsPanel.cs
    │       └── MessagesPanel.cs
    ├── Tests/
    │   ├── [Priority folders organized below]
    ├── Utilities/
    │   ├── BrowserHelpers.cs
    │   ├── WaitHelpers.cs
    │   └── AssertionHelpers.cs
    └── FacilitationAssistant.E2ETests.csproj
```

---

## 2. Test Prioritization

### P0 - Critical (Must Pass for Release)
Tests that validate core functionality without which the application is unusable.

### P1 - High (Should Pass for Release)
Tests that validate important features that significantly impact user experience.

### P2 - Medium (Nice to Have)
Tests that validate edge cases and less critical features.

### P3 - Low (Enhancement)
Tests for polish features, accessibility, and performance metrics.

---

## 3. Test Cases by Feature

### 3.1 Meeting Creation & Link Management (P0)

#### Test Class: `MeetingCreationTests.cs`

**P0-001: Create Meeting and Generate Links**
- **Given**: User is on homepage
- **When**: User clicks "Create New Meeting"
- **Then**: 
  - Meeting is created with unique ID
  - Facilitator link is displayed and copyable
  - Attendee link is displayed and copyable
  - Both links contain unique, non-sequential identifiers (≥16 chars)
  - Visual distinction between facilitator and attendee links

**P0-002: Facilitator Link Access Control**
- **Given**: Meeting is created
- **When**: User accesses facilitator link
- **Then**: 
  - Facilitator interface loads with full controls
  - Agenda builder is visible
  - "Start Meeting" button is present
  - Timer controls are visible

**P0-003: Attendee Link Access Control**
- **Given**: Meeting is created but not started
- **When**: User accesses attendee link
- **Then**: 
  - "Meeting not started yet" message is displayed
  - Facilitator controls are not visible
  - Agenda is not visible yet

**P0-004: Attendee Link Activated After Meeting Start**
- **Given**: Facilitator has created meeting and added stages
- **When**: Facilitator clicks "Start Meeting"
- **Then**: 
  - Attendee accessing link sees full agenda
  - Stage information is visible
  - No facilitator controls are present

**P1-005: Link Persistence Across Sessions**
- **Given**: Meeting is created
- **When**: User closes browser and reopens facilitator link
- **Then**: Meeting state is persisted and accessible

**P1-006: Link Security - Attendee Cannot Access Facilitator Functions**
- **Given**: User has attendee link
- **When**: User attempts to access facilitator API endpoints directly
- **Then**: All requests return 403 Forbidden

**P2-007: Link Collision Prevention**
- **Given**: Multiple meetings are created rapidly
- **When**: 10 meetings are created in parallel
- **Then**: All meeting IDs are unique

---

### 3.2 Agenda Management & Real-Time Timing (P0)

#### Test Class: `AgendaManagementTests.cs`

**P0-010: Add Agenda Stages**
- **Given**: Facilitator is in meeting setup
- **When**: Facilitator adds multiple stages with names and durations
- **Then**: 
  - All stages appear in correct order
  - Stage names and durations are displayed correctly
  - Minimum 1 stage required before starting meeting

**P0-011: Start Stage Timer**
- **Given**: Meeting is active with stages added
- **When**: Facilitator clicks "Start" on a stage
- **Then**: 
  - Stage becomes active with "Active" status
  - Timer starts counting up
  - Progress bar begins animating
  - All attendees see active stage within 2 seconds

**P0-012: End Stage and Timer Stop**
- **Given**: Stage is currently active
- **When**: Facilitator clicks "End Stage"
- **Then**: 
  - Stage status changes to "Completed"
  - Timer stops
  - Actual duration is displayed
  - All attendees see completed stage within 2 seconds

**P0-013: Real-Time Stage Sync to Attendees**
- **Given**: Facilitator and attendee are both connected
- **When**: Facilitator transitions to a new stage
- **Then**: 
  - Attendee sees stage change within 2 seconds
  - Attendee's timer matches facilitator's timer (±1 sec)
  - Active stage is highlighted for attendee

**P0-014: Overall Meeting Timer**
- **Given**: Meeting has started
- **When**: User views the interface
- **Then**: 
  - Total meeting elapsed time is displayed
  - Format shows hours and minutes correctly
  - Timer updates every second

**P1-015: Stage Overrun Indicator**
- **Given**: Stage duration exceeds planned time
- **When**: User views the stage
- **Then**: 
  - "Overrunning by Xm Ys" message appears
  - Progress bar extends beyond 100% in warning color
  - Warning color (red/orange) is applied

**P1-016: Reorder Agenda Stages**
- **Given**: Facilitator has added multiple stages
- **When**: Facilitator drags a stage to new position
- **Then**: 
  - Stage order updates correctly
  - All stage data is preserved
  - Stage numbers update sequentially

**P1-017: Skip Stage by Starting Later Stage**
- **Given**: Multiple stages exist, Stage 1 is active
- **When**: Facilitator starts Stage 3 directly
- **Then**: 
  - Stage 1 ends with "Completed" status
  - Stage 2 shows "Not Started" or "Skipped" status
  - Stage 3 becomes active

**P2-018: Extend Stage Duration On-the-Fly**
- **Given**: Stage is active and overrunning
- **When**: Facilitator clicks "Add 5 Minutes"
- **Then**: 
  - Planned duration increases by 5 minutes
  - Progress bar adjusts
  - Overrun warning disappears if new duration > elapsed time

**P2-019: Browser Tab Inactive Timer Sync**
- **Given**: Attendee has stage active
- **When**: Attendee switches to another tab for 30 seconds then returns
- **Then**: Timer catches up to correct time without freeze

---

### 3.3 Questions & Polling System (P0)

#### Test Class: `PollingSystemTests.cs`

**P0-020: Create Single-Choice Question**
- **Given**: Facilitator is in meeting setup or active meeting
- **When**: Facilitator creates single-choice question with options
- **Then**: 
  - Question is saved with all options
  - Answer type is set to "single choice"
  - Question appears in facilitator's question list

**P0-021: Trigger Question to Attendees**
- **Given**: Facilitator has created a question
- **When**: Facilitator triggers question "Show Immediately"
- **Then**: 
  - All attendees see question modal within 2 seconds
  - Question text and options are displayed correctly
  - Submit and Skip buttons are present

**P0-022: Attendee Submits Single-Choice Answer**
- **Given**: Question is displayed to attendee
- **When**: Attendee selects an option and clicks Submit
- **Then**: 
  - Answer is submitted successfully
  - "Answer submitted" confirmation appears
  - Question modal dismisses
  - Attendee cannot change answer

**P0-023: Facilitator Views Real-Time Results**
- **Given**: Multiple attendees have answered question
- **When**: Facilitator views results
- **Then**: 
  - Total responses count is displayed
  - Percentage breakdown per option is shown
  - Results update within 5 seconds as new responses arrive

**P1-024: Create Multiple-Choice Question**
- **Given**: Facilitator is creating a question
- **When**: Facilitator selects multiple-choice type and defines max selections
- **Then**: 
  - Question allows up to max selections
  - Validation prevents exceeding max

**P1-025: Create Free-Text Question**
- **Given**: Facilitator is creating a question
- **When**: Facilitator selects free-text type
- **Then**: 
  - Question displays textarea to attendees
  - Character count (max 1000) is visible
  - Attendees can submit text answers

**P1-026: Create Scale Question (1-5 Rating)**
- **Given**: Facilitator is creating a question
- **When**: Facilitator selects scale type with range 1-5
- **Then**: 
  - Attendees see slider or radio buttons
  - Results show average score and distribution

**P1-027: Attendee Skips Question**
- **Given**: Question is displayed to attendee
- **When**: Attendee clicks "Skip"
- **Then**: 
  - Question dismisses without submission
  - Facilitator sees attendee in "skipped" count

**P1-028: Question Queue Management**
- **Given**: Facilitator triggers 3 questions simultaneously
- **When**: Attendee views interface
- **Then**: 
  - Questions appear one at a time (FIFO)
  - Indicator shows "X questions pending"
  - Each question can be answered in sequence

**P1-029: Repeated Question at Different Stages**
- **Given**: Question was asked in Stage 1
- **When**: Facilitator asks same question again in Stage 3
- **Then**: 
  - Question is treated as new instance
  - Attendees can answer again independently
  - Results are segmented by stage

**P2-030: Question Template Library**
- **Given**: Facilitator is adding a question
- **When**: Facilitator clicks "Use Template"
- **Then**: 
  - Template library appears with common questions
  - Selecting template pre-fills question text
  - Facilitator can edit pre-filled content

---

### 3.4 Concerns & Feedback System (P1)

#### Test Class: `ConcernsFeedbackTests.cs`

**P1-040: Attendee Raises Predefined Concern**
- **Given**: Attendee is in active meeting
- **When**: Attendee clicks "Raise Concern" and selects predefined option
- **Then**: 
  - Concern is submitted successfully
  - Concern appears in concerns panel within 2 seconds
  - Facilitator sees concern with "New" status

**P1-041: Attendee Raises Custom Concern**
- **Given**: Attendee clicks "Raise Concern"
- **When**: Attendee selects "Custom Reason" and enters text (10-200 chars)
- **Then**: 
  - Custom concern is submitted
  - Text is displayed to facilitator and other attendees
  - Concern appears in panel with timestamp

**P1-042: Attendee Votes on Concern**
- **Given**: Concern exists in panel
- **When**: Attendee clicks Like/Dislike/Neutral
- **Then**: 
  - Vote is registered
  - Vote count increments by 1
  - Vote updates appear to all users within 2 seconds

**P1-043: Attendee Changes Vote**
- **Given**: Attendee has voted "Like" on a concern
- **When**: Attendee clicks "Dislike"
- **Then**: 
  - Previous vote is replaced
  - Like count decrements, Dislike count increments
  - Only one vote per attendee per concern

**P1-044: Facilitator Acknowledges Concern**
- **Given**: Concern has "New" status
- **When**: Facilitator clicks "Acknowledge"
- **Then**: 
  - Concern status changes to "Acknowledged"
  - Highlight is removed
  - All users see "Acknowledged by Facilitator" label

**P1-045: Facilitator Responds to Concern**
- **Given**: Concern exists
- **When**: Facilitator clicks "Respond" and types response
- **Then**: 
  - Response text (max 500 chars) is saved
  - All users see response below concern
  - Response is editable/deletable by facilitator

**P1-046: Attendee Withdraws Concern**
- **Given**: Attendee previously raised a concern
- **When**: Attendee clicks "Withdraw" on their concern
- **Then**: 
  - Concern is marked "Withdrawn" and grayed out
  - Other users can still see original text and votes
  - Concern cannot receive new votes

**P2-047: Concern Rate Limiting**
- **Given**: Attendee is raising concerns rapidly
- **When**: Attendee tries to raise 6th concern within 5 minutes
- **Then**: 
  - Warning "Slow down, please" is displayed
  - Rate limit prevents submission (max 5 per 5 min)

**P2-048: Concerns Panel Visibility**
- **Given**: User is viewing meeting interface
- **When**: User toggles concerns panel
- **Then**: 
  - Panel opens/closes without blocking agenda
  - Badge shows count of unacknowledged concerns
  - Panel is accessible as sidebar or tab

---

### 3.5 Notes System (P1)

#### Test Class: `NotesSystemTests.cs`

**P1-050: Facilitator Creates Public Note**
- **Given**: Facilitator is in active meeting
- **When**: Facilitator adds note with visibility "Public"
- **Then**: 
  - Note is saved with rich text formatting
  - Note appears in notes panel immediately
  - All attendees can see the note

**P1-051: Facilitator Creates Private Note**
- **Given**: Facilitator is in active meeting
- **When**: Facilitator adds note with visibility "Private"
- **Then**: 
  - Note is saved with "🔒 Private" icon
  - Only facilitator can see the note
  - Attendees do not see the private note

**P1-052: Attendee Creates Own Note**
- **Given**: Attendee is in active meeting
- **When**: Attendee adds personal note
- **Then**: 
  - Note is saved and visible to attendee
  - Facilitator can see public attendee notes (if enabled)
  - Note persists through session

**P1-053: Associate Note with Stage**
- **Given**: Facilitator is viewing a specific stage
- **When**: Facilitator adds note to that stage
- **Then**: 
  - Note is associated with stage
  - Note appears under stage section in notes panel
  - Note grouping by stage is maintained

**P1-054: Create Meeting-Level Note**
- **Given**: Facilitator is in meeting
- **When**: Facilitator adds note not associated with any stage
- **Then**: 
  - Note appears in "General Meeting Notes" section
  - Note is not stage-specific

**P1-055: Edit Note**
- **Given**: User has created a note
- **When**: User clicks "Edit" and modifies content
- **Then**: 
  - Updated content replaces original
  - Other users see updated content within 5 seconds
  - "Last edited at [time]" timestamp is updated

**P1-056: Delete Note**
- **Given**: User has created a note
- **When**: User clicks "Delete" and confirms
- **Then**: 
  - Note is removed from all participants' views within 2 seconds
  - Note does not appear in post-meeting summary

**P1-057: Rich Text Formatting**
- **Given**: User is creating/editing note
- **When**: User applies formatting (bold, italic, lists, links)
- **Then**: 
  - Formatting is preserved in display
  - Links are clickable and open in new tab
  - Bullet/numbered lists render correctly

**P2-058: Note Auto-Save Draft**
- **Given**: User is typing a note
- **When**: User closes browser before saving
- **Then**: 
  - Draft is auto-saved to local storage every 10 seconds
  - Draft recovers on next session (future enhancement)

**P2-059: Change Note Visibility**
- **Given**: Facilitator has a private note
- **When**: Facilitator changes visibility to "Public"
- **Then**: 
  - Confirmation dialog appears
  - Note becomes visible to all attendees after confirmation

---

### 3.6 Messaging & Communication System (P1)

#### Test Class: `MessagingSystemTests.cs`

**P1-060: Facilitator Sends Announcement**
- **Given**: Facilitator is in active meeting
- **When**: Facilitator sends message type "Announcement"
- **Then**: 
  - All attendees receive notification within 2 seconds
  - Message appears as banner/modal
  - Only "Dismiss" button is present (no response mechanism)

**P1-061: Facilitator Sends Question with Reactions**
- **Given**: Facilitator is composing a message
- **When**: Facilitator selects "Question" with response type "Reactions"
- **Then**: 
  - Message is sent to all attendees
  - Attendees see reaction buttons (👍 👎 ❤️ 😂 😮)
  - Reactions can be submitted

**P1-062: Facilitator Sends Question with Predefined Answers**
- **Given**: Facilitator creates question message
- **When**: Facilitator defines 2-6 answer options
- **Then**: 
  - Options appear as clickable buttons to attendees
  - Attendees can select one option and submit
  - Results show answer distribution

**P1-063: Attendee Responds with Reaction**
- **Given**: Message with reactions is displayed
- **When**: Attendee clicks a reaction
- **Then**: 
  - Reaction is registered
  - Reaction count increments
  - Facilitator sees updated counts within 5 seconds

**P1-064: Attendee Responds with Free Text**
- **Given**: Message has free-text response type
- **When**: Attendee types response (max 500 chars) and submits
- **Then**: 
  - Text is submitted to facilitator
  - Attendee sees "Response recorded" confirmation
  - Attendee cannot edit after submission

**P1-065: Message History**
- **Given**: Multiple messages have been sent
- **When**: Attendee clicks "Messages" icon
- **Then**: 
  - Scrollable list of all messages in chronological order
  - Each message shows text, timestamp, response count
  - Attendee's responses are indicated

**P1-066: Message Auto-Dismiss Notification**
- **Given**: Message notification appears
- **When**: Attendee does not interact for 10 seconds
- **Then**: 
  - Notification auto-dismisses
  - Message moves to message history
  - Attendee can still respond from history

**P1-067: Close Message to Stop Responses**
- **Given**: Facilitator has sent a message
- **When**: Facilitator clicks "Close Message"
- **Then**: 
  - Response options are disabled for attendees
  - Message shows "Responses closed"
  - Final results are visible

**P2-068: Message Queue (Multiple Rapid Messages)**
- **Given**: Facilitator sends 3 messages rapidly
- **When**: Attendees view interface
- **Then**: 
  - Messages queue and display one at a time (FIFO)
  - Indicator shows "3 messages pending"

**P2-069: Message Rate Limiting**
- **Given**: Facilitator is sending messages rapidly
- **When**: Facilitator tries to send 6th message within 5 minutes
- **Then**: 
  - Rate limit warning appears
  - Prevents spam (5 messages per 5 min)

---

### 3.7 Meeting Summary & Post-Meeting Access (P1)

#### Test Class: `MeetingSummaryTests.cs`

**P1-070: End Meeting and Generate Summary**
- **Given**: Meeting is active
- **When**: Facilitator clicks "End Meeting" and confirms
- **Then**: 
  - All timers stop
  - Meeting status changes to "Ended"
  - Both links redirect to read-only summary

**P1-071: Summary Shows Agenda Overview**
- **Given**: Meeting has ended
- **When**: User accesses summary
- **Then**: 
  - All stages are listed with status (Completed/Skipped)
  - Planned vs actual durations are displayed
  - Total meeting duration is shown

**P1-072: Summary Shows Public Notes (Facilitator View)**
- **Given**: Facilitator accesses summary after meeting end
- **When**: Facilitator views notes section
- **Then**: 
  - All public notes and private notes are visible
  - Notes are organized by stage
  - General meeting notes are in separate section

**P1-073: Summary Shows Public Notes (Attendee View)**
- **Given**: Attendee accesses summary after meeting end
- **When**: Attendee views notes section
- **Then**: 
  - Facilitator's public notes are visible
  - Attendee's own notes (public/private) are visible
  - Other attendees' private notes are NOT visible

**P1-074: Summary Shows Poll Results**
- **Given**: Polls were conducted during meeting
- **When**: User views summary
- **Then**: 
  - All questions are listed with results
  - Aggregated percentages and counts are displayed
  - Free-text responses are shown (with anonymization)

**P1-075: Summary Shows Concerns**
- **Given**: Concerns were raised during meeting
- **When**: User views summary
- **Then**: 
  - All concerns are listed with vote counts
  - Facilitator responses are displayed
  - Withdrawn concerns are marked appropriately

**P1-076: Summary Shows Messages**
- **Given**: Messages were sent during meeting
- **When**: User views summary
- **Then**: 
  - All messages are listed chronologically
  - Response results are aggregated and displayed

**P1-077: Print/Export Summary**
- **Given**: User is viewing summary
- **When**: User clicks "Print" or uses browser print
- **Then**: 
  - Print-friendly version formats correctly
  - No interactive elements in print view
  - Content flows across pages without cut-offs

**P2-078: Summary URL Stability**
- **Given**: Meeting has ended
- **When**: User bookmarks facilitator/attendee link
- **Then**: 
  - Bookmark remains valid
  - Opens summary view directly

---

### 3.8 Onboarding & First-Run Experience (P2)

#### Test Class: `OnboardingTests.cs`

**P2-080: Facilitator First-Time Tour**
- **Given**: User creates first meeting (localStorage empty)
- **When**: Dashboard loads
- **Then**: 
  - "Welcome Tour" overlay appears
  - Tour highlights: Meeting Link, Timer Controls, Feedback Panel
  - Tour can be dismissed

**P2-081: Attendee First-Time Welcome**
- **Given**: Attendee joins meeting for first time (localStorage empty)
- **When**: Page loads
- **Then**: 
  - "Welcome" modal appears
  - Shows anonymity status ("Attendee #X")
  - Explains facilitator controls agenda

**P2-082: Tour Persistence**
- **Given**: User has dismissed tour
- **When**: User refreshes page or returns to site
- **Then**: 
  - Tour does not reappear
  - Tour state is stored in localStorage

**P2-083: Manual Tour Restart**
- **Given**: User has dismissed tour
- **When**: User clicks "Help" or "?" icon in header
- **Then**: 
  - Tour can be manually restarted
  - All tour steps are accessible again

---

### 3.9 User Settings (P2)

#### Test Class: `UserSettingsTests.cs`

**P2-090: Access Settings Modal**
- **Given**: User is in meeting
- **When**: User clicks settings icon/gear
- **Then**: 
  - Settings modal opens
  - All settings categories are accessible

**P2-091: Change Display Name**
- **Given**: User opens settings
- **When**: User changes display name (optional feature)
- **Then**: 
  - Name is saved and displayed
  - Name persists across sessions (localStorage)

**P2-092: Toggle Notifications**
- **Given**: User opens settings
- **When**: User toggles notification settings
- **Then**: 
  - Notification preferences are saved
  - Future notifications respect preferences

**P2-093: Change Theme (Light/Dark Mode)**
- **Given**: User opens settings
- **When**: User switches between light/dark theme
- **Then**: 
  - Theme changes immediately
  - Theme preference persists (localStorage)

**P2-094: Font Size Adjustment**
- **Given**: User opens settings
- **When**: User adjusts font size (accessibility)
- **Then**: 
  - Font size changes across interface
  - Preference persists

---

### 3.10 Real-Time Synchronization & State Management (P0)

#### Test Class: `RealTimeSyncTests.cs`

**P0-100: SignalR Connection Establishment**
- **Given**: User accesses meeting link
- **When**: Page loads
- **Then**: 
  - SignalR connection establishes within 3 seconds
  - "Connected" indicator appears (green dot)
  - WebSocket connection is active

**P0-101: Fallback to Long Polling**
- **Given**: WebSocket connection fails
- **When**: Fallback mechanism activates
- **Then**: 
  - Long polling mode activates
  - "Limited connectivity" warning appears (yellow indicator)
  - Basic functionality continues

**P0-102: Disconnection Detection**
- **Given**: User is connected to meeting
- **When**: Network connection is lost
- **Then**: 
  - "Disconnected - Reconnecting..." message appears
  - Retry countdown is visible
  - All facilitator controls are disabled

**P0-103: Automatic Reconnection**
- **Given**: User was disconnected
- **When**: Network returns
- **Then**: 
  - Reconnection attempts occur (1s, 2s, 5s, 10s, 30s intervals)
  - Full meeting state snapshot is received on reconnect
  - Interface resyncs with current state

**P0-104: Server-Side Time Synchronization**
- **Given**: Attendee's system clock is incorrect (±5 minutes)
- **When**: Timers are displayed
- **Then**: 
  - Timers use server-authoritative timestamp
  - Elapsed time is calculated server-side
  - Timer accuracy is independent of client clock

**P1-105: Concurrent Facilitator Actions**
- **Given**: Facilitator has two browser tabs open
- **When**: Different actions are triggered in each tab simultaneously
- **Then**: 
  - Last action by server timestamp wins
  - Both tabs sync to same state
  - No data loss or conflicts

**P1-106: Attendee Join/Leave Notifications**
- **Given**: Facilitator is viewing meeting
- **When**: Attendee joins or leaves
- **Then**: 
  - Facilitator sees "X attendees connected" count update within 2 seconds
  - Count reflects current connections

**P2-107: Browser Tab Throttling Handling**
- **Given**: Attendee switches to another browser tab
- **When**: Tab becomes inactive for extended period
- **Then**: 
  - SignalR circuit maintains connection (per timeout config)
  - On tab reactivation, state resyncs via OnAfterRender

**P2-108: Split-Brain Prevention**
- **Given**: Facilitator's connection drops
- **When**: Facilitator attempts actions while disconnected
- **Then**: 
  - Aggressive "Disconnected" warning banner appears
  - All controls are disabled
  - Actions are prevented until reconnection

---

### 3.11 Cross-Feature Integration Tests (P1)

#### Test Class: `IntegrationTests.cs`

**P1-110: Complete Meeting Workflow**
- **Given**: New meeting is created
- **When**: Complete meeting lifecycle is executed:
  1. Create meeting
  2. Add agenda stages
  3. Start meeting
  4. Transition through stages
  5. Raise concerns and questions
  6. Create notes
  7. Send messages
  8. End meeting
- **Then**: 
  - All features work together seamlessly
  - Summary contains all data
  - No data loss throughout workflow

**P1-111: Multi-User Concurrent Interactions**
- **Given**: 1 facilitator and 5 attendees are connected
- **When**: All users interact simultaneously:
  - Facilitator transitions stages
  - Attendees answer questions
  - Attendees raise concerns
  - All users take notes
- **Then**: 
  - All actions are processed correctly
  - No race conditions or conflicts
  - Data integrity is maintained

**P1-112: Stage Transition During Active Question**
- **Given**: Question is displayed to attendees
- **When**: Facilitator transitions to next stage while question is active
- **Then**: 
  - Question remains accessible
  - Attendees can still submit responses
  - Question response is saved correctly

**P1-113: Note Creation During Stage Transition**
- **Given**: User is typing a note
- **When**: Facilitator ends current stage
- **Then**: 
  - Note save completes successfully
  - Note is associated with correct stage
  - No data loss occurs

---

### 3.12 Error Handling & Recovery (P1)

#### Test Class: `ErrorHandlingTests.cs`

**P1-120: Invalid Meeting ID Access**
- **Given**: User attempts to access non-existent meeting ID
- **When**: URL with invalid ID is accessed
- **Then**: 
  - "Meeting not found" error page is displayed
  - User is redirected to homepage
  - No application crash

**P1-121: Network Timeout During Action**
- **Given**: User submits an action (e.g., raise concern)
- **When**: Network request times out
- **Then**: 
  - "Submitting..." state is shown
  - Retry logic attempts resubmission
  - User sees success confirmation or error message

**P1-122: Database Connection Failure**
- **Given**: Application is running
- **When**: Database connection is lost
- **Then**: 
  - Graceful error handling
  - User sees "Service temporarily unavailable" message
  - Application does not crash

**P1-123: Malformed Input Validation**
- **Given**: User enters data in form fields
- **When**: User enters invalid data (e.g., special characters, excessive length)
- **Then**: 
  - Client-side validation prevents submission
  - Clear error messages are displayed
  - No server errors occur

**P2-124: SignalR Connection Lost During High Activity**
- **Given**: Meeting has high real-time activity
- **When**: SignalR connection drops due to network issue
- **Then**: 
  - Reconnection logic activates
  - Users are notified of disconnection
  - State resyncs correctly after reconnection

---

### 3.13 Security & Access Control (P0)

#### Test Class: `SecurityTests.cs`

**P0-130: Facilitator Link Secrecy**
- **Given**: Meeting is created
- **When**: Links are generated
- **Then**: 
  - Facilitator and attendee links are different
  - Links contain cryptographically secure random identifiers (≥16 chars)
  - No sequential or guessable patterns

**P0-131: Attendee Cannot Access Facilitator Actions via API**
- **Given**: Attendee has attendee link
- **When**: Attendee sends API requests to facilitator-only endpoints
- **Then**: 
  - All requests return 403 Forbidden
  - No unauthorized actions are executed

**P0-132: XSS Prevention in User-Generated Content**
- **Given**: User enters content with HTML/script tags
- **When**: Content is displayed (notes, concerns, messages)
- **Then**: 
  - Input is sanitized
  - Dangerous tags are stripped
  - Safe formatting is preserved

**P0-133: CSRF Protection**
- **Given**: Application uses anti-forgery tokens
- **When**: State-changing requests are made
- **Then**: 
  - Tokens are validated
  - Requests without valid tokens are rejected

**P1-134: Rate Limiting Enforcement**
- **Given**: Application has rate limits configured
- **When**: User exceeds rate limit (60 req/min)
- **Then**: 
  - 429 Too Many Requests response is returned
  - "Too many requests. Please try again later." message is displayed

---

### 3.14 Accessibility (P2)

#### Test Class: `AccessibilityTests.cs`

**P2-140: Keyboard Navigation**
- **Given**: User navigates interface with keyboard only
- **When**: User presses Tab/Shift+Tab to navigate
- **Then**: 
  - All interactive elements are accessible
  - Focus order is logical
  - Focus indicators are visible

**P2-141: Screen Reader Compatibility**
- **Given**: User uses screen reader (e.g., NVDA, JAWS)
- **When**: User navigates meeting interface
- **Then**: 
  - All content is announced correctly
  - ARIA labels are present
  - Dynamic content updates are announced

**P2-142: Color Contrast Compliance**
- **Given**: User views interface
- **When**: Color contrast is measured
- **Then**: 
  - All text meets WCAG AA standards (4.5:1 contrast ratio)
  - Status indicators have sufficient contrast

**P2-143: Responsive Design - Mobile Devices**
- **Given**: User accesses application on mobile device
- **When**: Interface is displayed on small screen
- **Then**: 
  - Layout is responsive and usable
  - Collapsed views and sticky headers work correctly
  - Touch interactions function properly

---

### 3.15 Performance & Load (P2)

#### Test Class: `PerformanceTests.cs`

**P2-150: Page Load Performance**
- **Given**: User accesses meeting page
- **When**: Page load time is measured
- **Then**: 
  - Initial page load completes within 3 seconds
  - Time to interactive is < 5 seconds

**P2-151: Multiple Attendees Load**
- **Given**: Meeting has 50 concurrent attendees (documented capacity)
- **When**: All attendees interact simultaneously
- **Then**: 
  - Server handles load without degradation
  - All real-time updates are delivered within SLA (2 seconds)

**P2-152: Large Data Sets Rendering**
- **Given**: Meeting has 100+ notes, 50 questions, 200 concerns
- **When**: User accesses summary page
- **Then**: 
  - Page renders within 5 seconds
  - Pagination/collapsible sections prevent slowdown

---

## 4. Test Data Management

### 4.1 Test Data Factory
Create `TestDataFactory.cs` to generate test data:
- **Meetings**: Random IDs, timestamps
- **Stages**: Names, durations, statuses
- **Questions**: Various types, options
- **Concerns**: Text, votes, statuses
- **Notes**: Rich text, visibility levels
- **Messages**: Types, responses

### 4.2 Database Management
- **Setup**: Create test database instance before test run
- **Cleanup**: Drop/recreate database between test classes
- **Isolation**: Each test class uses isolated database or transactions
- **Seeding**: Seed common data (e.g., templates) before tests

---

## 5. Page Object Model (POM) Design

### 5.1 Base Page Object
```csharp
public abstract class BasePage
{
    protected IPage Page;
    protected string BaseUrl;
    
    public BasePage(IPage page, string baseUrl)
    {
        Page = page;
        BaseUrl = baseUrl;
    }
    
    public async Task WaitForLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
```

### 5.2 HomePage
- **Methods**: `ClickCreateMeeting()`, `GetFacilitatorLink()`, `GetAttendeeLink()`

### 5.3 FacilitatorPage
- **Methods**: 
  - `AddAgendaStage(name, duration)`
  - `StartMeeting()`
  - `StartStage(stageIndex)`
  - `EndStage()`
  - `CreateQuestion(questionData)`
  - `SendMessage(messageData)`
  - `ViewConcerns()`
  - `EndMeeting()`

### 5.4 AttendeePage
- **Methods**: 
  - `WaitForMeetingStart()`
  - `GetActiveStageText()`
  - `AnswerQuestion(answer)`
  - `RaiseConcern(concernText)`
  - `VoteOnConcern(concernId, voteType)`
  - `CreateNote(noteText)`

### 5.5 SummaryPage
- **Methods**: 
  - `GetAgendaStatus()`
  - `GetNotes()`
  - `GetPollResults()`
  - `GetConcerns()`
  - `PrintSummary()`

---

## 6. CI/CD Integration

### 6.1 Test Execution Strategy
- **On Pull Request**: Run P0 tests (critical path)
- **On Merge to Main**: Run P0 + P1 tests
- **Nightly**: Run all tests (P0, P1, P2, P3)

### 6.2 Parallel Execution
- Configure xUnit to run test classes in parallel
- Each test class uses isolated browser context
- Parallelization degree: 4 (adjustable based on CI resources)

### 6.3 Test Reporting
- Generate HTML test report with screenshots on failure
- Upload artifacts (screenshots, videos, traces) for failed tests
- Integrate with CI dashboard for visibility

---

## 7. Test Environment Setup

### 7.1 Prerequisites
- .NET 9.0 SDK
- Playwright CLI installed (`pwsh bin/Debug/net9.0/playwright.ps1 install`)
- PostgreSQL test instance or In-Memory database
- Test application running on `http://localhost:5000` (configurable)

### 7.2 Configuration
```json
{
  "TestSettings": {
    "BaseUrl": "http://localhost:5000",
    "BrowserType": "chromium",
    "Headless": true,
    "SlowMo": 0,
    "Timeout": 30000,
    "VideoOnFailure": true,
    "ScreenshotOnFailure": true
  }
}
```

---

## 8. Success Metrics

### 8.1 Coverage Goals
- **P0 Tests**: 100% pass rate required for release
- **P1 Tests**: 95% pass rate required for release
- **P2 Tests**: 85% pass rate acceptable
- **P3 Tests**: 70% pass rate acceptable

### 8.2 Performance Benchmarks
- Test execution time: < 20 minutes for full suite
- P0 suite execution time: < 5 minutes
- Flakiness rate: < 2% (tests flagged as flaky if fail <3% of runs)

---

## 9. Known Limitations & Future Enhancements

### 9.1 Current Limitations
- Tests assume single machine execution (no distributed testing yet)
- Browser types: Primary focus on Chromium; Firefox/WebKit as secondary
- Mobile testing: Emulation only (no real device testing)

### 9.2 Future Enhancements
- Visual regression testing for UI changes
- API testing layer (validate backend independently)
- Performance profiling and bottleneck detection
- Penetration testing scenarios
- Multi-language/localization testing

---

## 10. Appendix

### 10.1 Test Naming Convention
Format: `P{Priority}_{FeatureArea}_{Scenario}_{ExpectedOutcome}`
Example: `P0_MeetingCreation_AccessFacilitatorLink_ShouldShowFullControls`

### 10.2 Test Tags
Use xUnit traits for categorization:
- `[Trait("Priority", "P0")]`
- `[Trait("Feature", "MeetingCreation")]`
- `[Trait("Type", "UI")]`

### 10.3 Debugging Tips
- Run tests with `Headless = false` to watch execution
- Use `await Page.PauseAsync()` to pause execution for inspection
- Enable video recording: `VideoOnFailure = true`
- Use Playwright Inspector: `PWDEBUG=1 dotnet test`

---

**End of Test Plan**
